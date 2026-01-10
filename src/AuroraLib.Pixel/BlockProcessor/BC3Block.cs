using AuroraLib.Pixel.Processing.Helper;
using System;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.BlockProcessor
{

    /// <summary>
    /// Implements BC3 (DXT5) block compression for RGBA images. BC3 is composed of a BC1-compressed RGB portion and a BC4-compressed alpha portion.
    /// </summary>
    public sealed class BC3Block<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IRGBA<byte>
    {
        private const int BPB = 8 * 2;

        /// <inheritdoc/>
        public int BlockWidth => BC1.BlockWidth;

        /// <inheritdoc/>
        public int BlockHeight => BC1.BlockHeight;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        public BC3Block() : this(new BC1Block<TColor>.BoundingBoxColorEncoder(0)) // (alphaThreshold: 0) Encoder ignores alpha.
        { }

        public BC3Block(BC1Block<TColor>.IColorSelector colorEncoder)
            => BC1 = new BC1Block<TColor>(colorEncoder);

        private readonly BC1Block<TColor> BC1;
        private readonly BC4UBlock<AToI<TColor, byte>> BC4a = new BC4UBlock<AToI<TColor, byte>>();

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            BC1.DecodeBlock(source.Slice(8), target, stride);
            Span<AToI<TColor, byte>> intensity = MemoryMarshal.Cast<TColor, AToI<TColor, byte>>(target);
            BC4a.DecodeBlock(source.Slice(0, 8), intensity, stride);
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            ReadOnlySpan<AToI<TColor, byte>> intensity = MemoryMarshal.Cast<TColor, AToI<TColor, byte>>(source);
            BC4a.EncodeBlock(intensity, target.Slice(0, 8), stride);
            BC1.EncodeBlock(source, target.Slice(8), stride);
        }

    }
}
