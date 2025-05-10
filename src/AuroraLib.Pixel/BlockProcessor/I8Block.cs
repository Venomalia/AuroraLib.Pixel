using AuroraLib.Pixel.PixelFormats;
using System;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Provides block-based encoding and decoding for 8-bit indexed color images using 8x4 blocks.
    /// </summary>
    public class I8Block : IBlockProcessor<I8>
    {
        private const int BWidth = 8, BHeight = 4, BPB = BWidth * BHeight;

        /// <inheritdoc/>
        public int BlockWidth => BWidth;

        /// <inheritdoc/>
        public int BlockHeight => BHeight;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<I8> target, int stride)
        {
            Span<byte> targetByte = MemoryMarshal.Cast<I8, byte>(target);
            int i = 0;
            for (int y = 0; y < BHeight; y++)
            {
                int dst = y * stride;
                for (int x = 0; x < BWidth; x++)
                {
                    targetByte[dst + x] = source[i++];
                }
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<I8> source, Span<byte> target, int stride)
        {
            ReadOnlySpan<byte> sourceByte = MemoryMarshal.Cast<I8, byte>(source);
            int i = 0;
            for (int y = 0; y < BHeight; y++)
            {
                int src = y * stride;
                for (int x = 0; x < BWidth; x++)
                {
                    target[i++] = sourceByte[src + x];
                }
            }
        }
    }

}
