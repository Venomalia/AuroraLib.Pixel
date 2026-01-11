using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// UYVY (YUV 4:2:2) block processor. This format is identical to the <see cref="YUY2{TColor}"/> format, except for the byte order.
    /// </summary>
    /// <typeparam name="TColor">YUV color type (8-bit components).</typeparam>
    public sealed class UYVY<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IYUV<byte>
    {
        /// <inheritdoc/>
        public int BlockWidth => 2;

        /// <inheritdoc/>
        public int BlockHeight => 1;

        /// <inheritdoc/>
        public int BytesPerBlock => 4;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int _)
        {
            target[0].U = target[1].U = source[1];  // U
            target[0].Y = source[1];                // Y0
            target[0].V = target[1].V = source[2];  // V
            target[1].Y = source[3];                // Y1
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int _)
        {
            // Take chroma from the first pixel
            target[0] = source[0].U;
            target[1] = source[0].Y;
            target[2] = source[0].V;
            target[3] = source[1].Y;
        }
    }
}
