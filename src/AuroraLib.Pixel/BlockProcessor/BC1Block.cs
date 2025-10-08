using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Numerics;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes BC1 (DXT1) 4x4 pixel blocks with optional alpha support.
    /// Each block is compressed to 8 bytes: 2 reference colors + 2-bit indices per pixel.
    /// </summary>
    public sealed class BC1Block<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IRGBA<byte>
    {
        /// <summary>
        /// Default selector for selecting the color endpoints when encoding blocks.
        /// </summary>
        public static IColorSelector DefaultSelector { get; set; } = new BoundingBoxColorEncoder();

        private const int BPB = 8, BlockSize = 4;
        /// <inheritdoc/>
        public int BlockWidth => BlockSize;

        /// <inheritdoc/>
        public int BlockHeight => BlockSize;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        public BC1Block() : this(DefaultSelector)
        { }

        public BC1Block(IColorSelector colorEncoder)
            => _colorEncoder = colorEncoder;

        private readonly IColorSelector _colorEncoder;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            Span<TColor> interpolatedColors = stackalloc TColor[4];

            RGB565 color0 = (RGB565)BinaryPrimitives.ReadUInt16LittleEndian(source);
            RGB565 color1 = (RGB565)BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(2));
            GetInterpolatedColours(color0, color1, interpolatedColors);
            uint colorIndex = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(4));

            for (int y = 0; y < BlockSize; y++)
            {
                int row = y * stride;
                for (int x = 0; x < BlockSize; x++)
                {
                    int idx = (int)(colorIndex & 0b11);
                    target[row + x] = interpolatedColors[idx];
                    colorIndex >>= 2;
                }
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            Span<TColor> interpolatedColors = stackalloc TColor[4];

            _colorEncoder.GetEndpoints(source, stride, out RGB565 color0, out RGB565 color1);
            GetInterpolatedColours(color0, color1, interpolatedColors);

            uint colorIndex = 0;
            int shift = 0;
            for (int y = 0; y < BlockSize; y++)
            {
                for (int x = 0; x < BlockSize; x++)
                {
                    int index = y * stride + x;
                    int bestIndex = GetColorIndex(source[index], interpolatedColors, _colorEncoder.AlphaThreshold);
                    colorIndex |= (uint)(bestIndex << shift);
                    shift += 2;
                }
            }
            BinaryPrimitives.WriteUInt16LittleEndian(target, (ushort)color0);
            BinaryPrimitives.WriteUInt16LittleEndian(target.Slice(2), (ushort)color1);
            BinaryPrimitives.WriteUInt32LittleEndian(target.Slice(4), colorIndex);
        }

        static void GetInterpolatedColours(RGB565 left, RGB565 right, Span<TColor> colors)
        {
            left.ToRGBA(ref colors[0]);
            right.ToRGBA(ref colors[1]);

            //Needs Alpha Color?
            if ((ushort)left > (ushort)right)
            {
                colors[2].R = (byte)((2 * left.R + right.R) / 3);
                colors[2].G = (byte)((2 * left.G + right.G) / 3);
                colors[2].B = (byte)((2 * left.B + right.B) / 3);
                colors[2].A = byte.MaxValue;

                colors[3].R = (byte)((left.R + 2 * right.R) / 3);
                colors[3].G = (byte)((left.G + 2 * right.G) / 3);
                colors[3].B = (byte)((left.B + 2 * right.B) / 3);
                colors[3].A = byte.MaxValue;
            }
            else
            {
                colors[2].R = (byte)((left.R + right.R) >> 1);
                colors[2].G = (byte)((left.G + right.G) >> 1);
                colors[2].B = (byte)((left.B + right.B) >> 1);
                colors[2].A = byte.MaxValue;
                colors[3].A = byte.MinValue;
            }
        }

        static int GetColorIndex(TColor pixel, ReadOnlySpan<TColor> colors, byte alphaThreshold)
        {
            int minDistance = int.MaxValue, colorIndex = 0;

            //Needs Alpha Color?
            if (pixel.A < alphaThreshold && colors[colors.Length - 1].A == 0)
                return colors.Length - 1;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A == 0)
                    continue;

                int distance = CalculateColorDistance(pixel, colors[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    colorIndex = i;
                }
            }

            return colorIndex;

            static int CalculateColorDistance(TColor c1, TColor c2)
            {
                int dr = c1.R - c2.R;
                int dg = c1.G - c2.G;
                int db = c1.B - c2.B;
                return dr * dr + dg * dg + db * db;
            }
        }

        /// <summary>
        /// Provides the two dominant colors (endpoints) of a 4x4 pixel block in RGB565 format.
        /// </summary>
        public interface IColorSelector
        {
            /// <summary>
            /// Alpha threshold for transparent pixels.
            /// </summary>
            byte AlphaThreshold { get; }

            /// <summary>
            /// Calculates the two dominant colors (endpoints) of a pixel block.
            /// </summary>
            /// <param name="pixels">Block pixel data</param>
            /// <param name="stride">Row width of the block</param>
            /// <param name="color0">First endpoint (lighter or higher value)</param>
            /// <param name="color1">Second endpoint (darker or lower value)</param>
            void GetEndpoints(ReadOnlySpan<TColor> pixels, int stride, out RGB565 color0, out RGB565 color1);
        }

        /// <summary>
        /// Quickly computes the RGB565 min and max colors of a 4x4 pixel block, representing its color bounding box.
        /// </summary>
        public sealed class BoundingBoxColorEncoder : IColorSelector
        {
            /// <inheritdoc/>
            public byte AlphaThreshold { get; }

            public BoundingBoxColorEncoder(byte alphaThreshold = 32)
                => AlphaThreshold = alphaThreshold;

            /// <inheritdoc/>
            public void GetEndpoints(ReadOnlySpan<TColor> pixels, int stride, out RGB565 color0, out RGB565 color1)
            {
                bool NeedsAlphaColor = false;
                Vector3 minVec = new Vector3(255);
                Vector3 maxVec = new Vector3(0);

                for (int y = 0; y < BlockSize; y++)
                {
                    for (int x = 0; x < BlockSize; x++)
                    {
                        int i = y * stride + x;

                        if (pixels[i].A < AlphaThreshold)
                        {
                            NeedsAlphaColor = true;
                            continue;
                        }

                        Vector3 pixelVec = new Vector3(pixels[i].R, pixels[i].G, pixels[i].B);
                        minVec = Vector3.Min(minVec, pixelVec);
                        maxVec = Vector3.Max(maxVec, pixelVec);
                    }
                }

                color0 = new RGB565((byte)minVec.X, (byte)minVec.Y, (byte)minVec.Z);
                color1 = new RGB565((byte)maxVec.X, (byte)maxVec.Y, (byte)maxVec.Z);

                //Needs Alpha Color?
                if (NeedsAlphaColor != ((ushort)color0 < (ushort)color1))
                {
                    (color0, color1) = (color1, color0);
                }
            }
        }
    }
}
