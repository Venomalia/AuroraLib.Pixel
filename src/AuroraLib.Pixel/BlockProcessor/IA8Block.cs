using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Provides encoding and decoding for 4x4 blocks of IA8 (Intensity + Alpha, 8 bits each) pixel data.
    /// </summary>
    public class IA8Block : IBlockProcessor<IA16>
    {
        private const int BlockSize = 4, BPB = BlockSize * BlockSize * 2;

        /// <inheritdoc/>
        public int BlockWidth => BlockSize;

        /// <inheritdoc/>
        public int BlockHeight => BlockSize;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<IA16> target, int stride)
        {
            ReadOnlySpan<ushort> sourceUshort = MemoryMarshal.Cast<byte, ushort>(source);
            Span<ushort> targetUshort = MemoryMarshal.Cast<IA16, ushort>(target);
            int i = 0;
            for (int y = 0; y < BlockSize; y++)
            {
                int dst = y * stride;
                for (int x = 0; x < BlockSize; x++)
                {
                    targetUshort[dst + x] = BinaryPrimitives.ReverseEndianness(sourceUshort[i++]);
                }
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<IA16> source, Span<byte> target, int stride)
        {
            ReadOnlySpan<ushort> sourceUshort = MemoryMarshal.Cast<IA16, ushort>(source);
            Span<ushort> targetUshort = MemoryMarshal.Cast<byte, ushort>(target);
            int i = 0;
            for (int y = 0; y < BlockSize; y++)
            {
                int src = y * stride;
                for (int x = 0; x < BlockSize; x++)
                {
                    targetUshort[i++] = BinaryPrimitives.ReverseEndianness(sourceUshort[src + x]);
                }
            }
        }
    }
}
