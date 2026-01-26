using AuroraLib.Pixel.PixelFormats;
using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Block processor for 4-bit indexed pixel data (I4).
    /// Each block represents an 8x1 pixel tile packed into 4 bytes, with two pixel indices stored per byte.
    /// </summary>
    public sealed class I4Block : IBlockProcessor<I4>
    {
        /// <inheritdoc/>
        public int BlockWidth => 8;

        /// <inheritdoc/>
        public int BlockHeight => 1;

        /// <inheritdoc/>
        public int BytesPerBlock => 4;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<I4> target, int _)
        {
            byte packed = source[0];
            target[0].I = (byte)(packed >> 4);
            target[1].I = (byte)(packed & 0b1111);
            packed = source[1];
            target[2].I = (byte)(packed >> 4);
            target[3].I = (byte)(packed & 0b1111);
            packed = source[2];
            target[4].I = (byte)(packed >> 4);
            target[5].I = (byte)(packed & 0b1111);
            packed = source[3];
            target[6].I = (byte)(packed >> 4);
            target[7].I = (byte)(packed & 0b1111);
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<I4> source, Span<byte> target, int _)
        {
            target[0] = (byte)((source[0].I << 4) | (source[1].I & 0b1111));
            target[1] = (byte)((source[2].I << 4) | (source[3].I & 0b1111));
            target[2] = (byte)((source[4].I << 4) | (source[5].I & 0b1111));
            target[3] = (byte)((source[6].I << 4) | (source[7].I & 0b1111));
        }
    }
}
