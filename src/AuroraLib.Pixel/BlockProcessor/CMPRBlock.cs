using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers.Binary;
using System.Numerics;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes CMPR (S3TC/DXT1-like) texture blocks used on GameCube and Wii.
    /// </summary>
    public class CMPRBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IRGBA<byte>
    {
        private const int BPB = 4 * BlockSize * BlockSize / 8, BlockSize = 8;

        /// <inheritdoc/>
        public int BlockWidth => BlockSize;

        /// <inheritdoc/>
        public int BlockHeight => BlockSize;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            Span<TColor> colors = stackalloc TColor[4];

            for (int subblock = 0; subblock < 4; subblock++)
            {
                int subblockX = (subblock & 1) * 4;
                int subblockY = (subblock >> 1) * 4;

                RGB565 color0 = (RGB565)BinaryPrimitives.ReadUInt16BigEndian(source.Slice(subblock * 8, 2));
                RGB565 color1 = (RGB565)BinaryPrimitives.ReadUInt16BigEndian(source.Slice(subblock * 8 + 2, 2));
                int colorIndexes = BinaryPrimitives.ReadInt32BigEndian(source.Slice(subblock * 8 + 4, 4));

                GetInterpolatedColours(color0, color1, colors);
                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int colorIndex = (colorIndexes >> ((15 - pixel) * 2)) & 3;
                    target[y * stride + x] = colors[colorIndex];
                }
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            Span<TColor> interpolatedColors = stackalloc TColor[4];
            Span<TColor> subblockPixels = stackalloc TColor[16];

            for (int subblock = 0; subblock < 4; subblock++)
            {
                int subblockX = (subblock & 1) * 4;
                int subblockY = (subblock >> 1) * 4;

                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        int srcX = subblockX + y;
                        int srcY = subblockY + x;
                        subblockPixels[x * 4 + y] = source[srcY * stride + srcX];
                    }
                }

                GetDominantColors(subblockPixels, out RGB565 color0, out RGB565 color1);
                GetInterpolatedColours(color0, color1, interpolatedColors);


                int colorIndexes = 0;
                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int index = y * stride + x;
                    int colorIndex = GetColorIndex(source[index], interpolatedColors);
                    colorIndexes |= colorIndex << ((15 - pixel) * 2);
                }

                BinaryPrimitives.WriteUInt16BigEndian(target.Slice(subblock * 8, 2), (ushort)color0);
                BinaryPrimitives.WriteUInt16BigEndian(target.Slice(subblock * 8 + 2, 2), (ushort)color1);
                BinaryPrimitives.WriteInt32BigEndian(target.Slice(subblock * 8 + 4, 4), colorIndexes);
            }
        }

        private static void GetInterpolatedColours(RGB565 left, RGB565 right, Span<TColor> colors)
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

        private static void GetDominantColors(Span<TColor> pixels, out RGB565 color0, out RGB565 color1)
        {
            bool NeedsAlphaColor = false;
            Vector3 minVec = new Vector3(float.MaxValue);
            Vector3 maxVec = new Vector3(float.MinValue);

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].A < 16)
                {
                    NeedsAlphaColor = true;
                    continue;
                }
                Vector3 pixelVec = new Vector3(pixels[i].R / 255f, pixels[i].G / 255f, pixels[i].B / 255f);
                minVec = Vector3.Min(minVec, pixelVec);
                maxVec = Vector3.Max(maxVec, pixelVec);
            }

            color0 = new RGB565((byte)(minVec.X * 255f), (byte)(minVec.Y * 255f), (byte)(minVec.Z * 255f));
            color1 = new RGB565((byte)(maxVec.X * 255f), (byte)(maxVec.Y * 255f), (byte)(maxVec.Z * 255f));

            //Needs Alpha Color?
            if ((NeedsAlphaColor && (ushort)(color0) > (ushort)(color1)) || (!NeedsAlphaColor && (ushort)(color0) < (ushort)(color1)))
            {
                (color0, color1) = (color1, color0);
            }
        }

        private static int GetColorIndex(TColor pixel, Span<TColor> colors)
        {
            float minDistance = float.MaxValue;
            int colorIndex = 0;

            //Needs Alpha Color?
            if (pixel.A < 16)
                return colors.Length - 1;

            Vector4 pixelVector = pixel.ToScaledVector4();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].A == 0)
                    continue;

                float distance = CalculateColorDistance(pixelVector, colors[i].ToScaledVector4());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    colorIndex = i;
                }
            }

            return colorIndex;
            static float CalculateColorDistance(Vector4 color1, Vector4 color2) => (color1 - color2).Length();
        }
    }
}
