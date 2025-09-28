using AuroraLib.Pixel.PixelFormats;
using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Provides encoding and decoding for 4x4 blocks of RGBA32 pixel data used on GameCube, Wii.
    /// </summary>
    public sealed class GxRGBA32Block : IBlockProcessor<RGBA32>
    {
        private const int BlockSize = 4, BPB = BlockSize * BlockSize * 4;

        /// <inheritdoc/>
        public int BlockWidth => BlockSize;
        /// <inheritdoc/>
        public int BlockHeight => BlockSize;
        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /*
         * The pixel data is separated into two groups:
         * A and R are encoded in the first group, and G and B are encoded in the second group.
         * The data is organized as follows:
         * ARARARARARARARAR
         * ARARARARARARARAR
         * GBGBGBGBGBGBGBGB
         * GBGBGBGBGBGBGBGB
        */

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<RGBA32> target, int stride)
        {
            int i = 0;
            for (int y = 0; y < BlockSize; y++)
            {
                int dst = y * stride;
                for (int x = 0; x < BlockSize; x++)
                {
                    ref RGBA32 col = ref target[dst + x];
                    col.A = source[i * 2];
                    col.R = source[i * 2 + 1];
                    col.G = source[i * 2 + 32];
                    col.B = source[i * 2 + 33];
                    i++;
                }
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<RGBA32> source, Span<byte> target, int stride)
        {
            int i = 0;
            for (int y = 0; y < BlockSize; y++)
            {
                int dst = y * stride;
                for (int x = 0; x < BlockSize; x++)
                {
                    RGBA32 col = source[dst + x];
                    target[i * 2] = col.A;
                    target[i * 2 + 1] = col.R;
                    target[i * 2 + 32] = col.G;
                    target[i * 2 + 33] = col.B;
                    i++;
                }
            }
        }
    }
}