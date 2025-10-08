using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes BC2 (DXT3) 4x4 pixel blocks with explicit alpha (4 bits) and BC1 color data.
    /// Each block is compressed to 16 bytes: 8 bytes for alpha + 8 bytes for color (BC1).
    /// </summary>
    public sealed class BC2Block<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IRGBA<byte>
    {
        private const int BPB = 8 * 2;

        /// <inheritdoc/>
        public int BlockWidth => BC1.BlockWidth;

        /// <inheritdoc/>
        public int BlockHeight => BC1.BlockHeight;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        public BC2Block() : this(new BC1Block<TColor>.BoundingBoxColorEncoder(0)) // (alphaThreshold: 0) Encoder ignores alpha.
        { }

        public BC2Block(BC1Block<TColor>.IColorSelector colorEncoder)
            => BC1 = new BC1Block<TColor>(colorEncoder);

        private readonly BC1Block<TColor> BC1;

        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            BC1.DecodeBlock(source.Slice(8), target, stride);

            for (int i = 0; i < 8; i++)
            {
                byte b = source[i];

                int row = (i / 2) * stride;
                int col = (2 * i) % 4;
                target[row + col].A = (byte)((b >> 4) * 17);
                target[row + col + 1].A = (byte)((b & 0x0F) * 17);
            }
        }

        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            for (int i = 0; i < 8; i++)
            {
                int row = (i / 2) * stride;
                int col = (2 * i) % 4;

                byte alpha0 = (byte)((source[row + col].A + 8) / 17);
                byte alpha1 = (byte)((source[row + col + 1].A + 8) / 17);

                target[i] = (byte)((alpha0 << 4) | alpha1);
            }

            BC1.EncodeBlock(source, target.Slice(8), stride);
        }
    }
}
