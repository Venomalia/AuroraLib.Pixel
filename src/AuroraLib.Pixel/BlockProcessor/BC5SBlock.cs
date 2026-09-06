using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing.Helper;
using System;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Represents a BC5 signed (ATI2/3Dc) compressed texture block processor. stores two independent single-channel values.
    /// Each 4x4 block is 16 bytes in size, composed of two 8-byte BC4 blocks.
    /// </summary>
    public sealed class BC5SBlock : IBlockProcessor<IA<sbyte>>
    {
        private const int BPB = 8 * 2;

        /// <inheritdoc/>
        public int BlockWidth => BC4.BlockWidth;

        /// <inheritdoc/>
        public int BlockHeight => BC4.BlockHeight;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        private readonly BC4SBlock<IA<sbyte>> BC4 = new BC4SBlock<IA<sbyte>>();
        private readonly BC4SBlock<AToI<IA<sbyte>, sbyte>> BC4a = new BC4SBlock<AToI<IA<sbyte>, sbyte>>();

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<IA<sbyte>> target, int stride)
        {
            BC4.DecodeBlock(source.Slice(8), target, stride);
            Span<AToI<IA<sbyte>, sbyte>> intensity = MemoryMarshal.Cast<IA<sbyte>, AToI<IA<sbyte>, sbyte>>(target);
            BC4a.DecodeBlock(source.Slice(0, 8), intensity, stride);
        }

        public void EncodeBlock(ReadOnlySpan<IA<sbyte>> source, Span<byte> target, int stride)
        {
            BC4.EncodeBlock(source, target.Slice(8), stride);
            ReadOnlySpan<AToI<IA<sbyte>, sbyte>> intensity = MemoryMarshal.Cast<IA<sbyte>, AToI<IA<sbyte>, sbyte>>(source);
            BC4a.EncodeBlock(intensity, target.Slice(0, 8), stride);
        }
    }
}
