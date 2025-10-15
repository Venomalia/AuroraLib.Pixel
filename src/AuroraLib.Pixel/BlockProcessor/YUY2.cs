using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// YUY2 (YUV 4:2:2) block processor.
    /// </summary>
    /// <typeparam name="TColor">YUV color type (8-bit components).</typeparam>
    public sealed class YUY2<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IYUV<byte>
    {
        /// <inheritdoc/>
        public int BlockWidth => 2;

        /// <inheritdoc/>
        public int BlockHeight => 1;

        /// <inheritdoc/>
        public int BytesPerBlock => 4;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            for (int i = 0; i < source.Length; i += 4)
            {
                int p = i / 2;
                target[p].Y = source[i];
                target[p].U = target[p + 1].U = source[i + 1];
                target[p + 1].Y = source[i + 2];
                target[p].V = target[p + 1].V = source[i + 3];
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            for (int i = 0, p = 0; i < source.Length; i += 2, p += 4)
            {
                target[p + 0] = source[i].Y;
                target[p + 1] = (byte)((source[i].U + source[i + 1].U) / 2);
                target[p + 2] = source[i + 1].Y;
                target[p + 3] = (byte)((source[i].V + source[i + 1].V) / 2);
            }
        }
    }
}
