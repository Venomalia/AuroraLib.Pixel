using AuroraLib.Pixel.Processing.Helper;
using System;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Represents a BC5 unsigned (ATI2/3Dc) compressed texture block processor. stores two independent single-channel values.
    /// Each 4x4 block is 16 bytes in size, composed of two 8-byte BC4 blocks.
    /// </summary>
    public sealed class BC5UBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IIntensity<byte>, IAlpha<byte>
    {
        private const int BPB = 8 * 2;

        /// <inheritdoc/>
        public int BlockWidth => BC4.BlockWidth;

        /// <inheritdoc/>
        public int BlockHeight => BC4.BlockHeight;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        private readonly BC4UBlock<TColor> BC4 = new BC4UBlock<TColor>();
        private readonly BC4UBlock<AToI<TColor,byte>> BC4a = new BC4UBlock<AToI<TColor, byte>>();

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            BC4.DecodeBlock(source.Slice(8), target, stride);
            Span<AToI<TColor, byte>> intensity = MemoryMarshal.Cast<TColor, AToI<TColor, byte>>(target);
            BC4a.DecodeBlock(source.Slice(0, 8), intensity, stride);
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            BC4.EncodeBlock(source, target.Slice(8), stride);
            ReadOnlySpan<AToI<TColor, byte>> intensity = MemoryMarshal.Cast<TColor, AToI<TColor, byte>>(source);
            BC4a.EncodeBlock(intensity, target.Slice(0, 8), stride);
        }
    }
}
