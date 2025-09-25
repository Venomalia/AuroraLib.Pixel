using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers.Binary;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes CMPR (S3TC/DXT1-like) texture blocks used on GameCube and Wii.
    /// </summary>
    public sealed class CMPRBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IRGBA<byte>
    {
        private const int BPB = 4 * 8, BlockSize = 8;

        /// <inheritdoc/>
        public int BlockWidth => BlockSize;

        /// <inheritdoc/>
        public int BlockHeight => BlockSize;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            Span<TColor> interpolatedColors = stackalloc TColor[4];

            for (int subblock = 0; subblock < 4; subblock++)
            {
                int subblockX = (subblock & 1) * 4;
                int subblockY = (subblock >> 1) * 4;

                RGB565 color0 = (RGB565)BinaryPrimitives.ReadUInt16BigEndian(source.Slice(subblock * 8, 2));
                RGB565 color1 = (RGB565)BinaryPrimitives.ReadUInt16BigEndian(source.Slice(subblock * 8 + 2, 2));
                int colorIndexes = BinaryPrimitives.ReadInt32BigEndian(source.Slice(subblock * 8 + 4, 4));

                BC1Block<TColor>.GetInterpolatedColours(color0, color1, interpolatedColors);
                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int colorIndex = (colorIndexes >> ((15 - pixel) * 2)) & 3;
                    target[y * stride + x] = interpolatedColors[colorIndex];
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

                BC1Block<TColor>.GetDominantColors(subblockPixels, out RGB565 color0, out RGB565 color1);
                BC1Block<TColor>.GetInterpolatedColours(color0, color1, interpolatedColors);


                int colorIndexes = 0;
                for (int pixel = 0; pixel < 16; pixel++)
                {
                    int x = subblockX + (pixel & 3);
                    int y = subblockY + (pixel >> 2);
                    int index = y * stride + x;
                    int colorIndex = BC1Block<TColor>.GetColorIndex(source[index], interpolatedColors);
                    colorIndexes |= colorIndex << ((15 - pixel) * 2);
                }

                BinaryPrimitives.WriteUInt16BigEndian(target.Slice(subblock * 8, 2), (ushort)color0);
                BinaryPrimitives.WriteUInt16BigEndian(target.Slice(subblock * 8 + 2, 2), (ushort)color1);
                BinaryPrimitives.WriteInt32BigEndian(target.Slice(subblock * 8 + 4, 4), colorIndexes);
            }
        }
    }
}
