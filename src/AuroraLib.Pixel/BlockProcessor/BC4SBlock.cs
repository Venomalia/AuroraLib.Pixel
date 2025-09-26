using System;
using System.Buffers.Binary;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes BC4 (DXT5) blocks with signed normalized data (-1.0 to 1.0 range, stored as SNORM).
    /// Each 4x4 block is compressed to 8 bytes: 2 reference values + 3-bit indices per pixel.
    /// </summary>
    public sealed class BC4SBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IIntensity<byte>
    {
        private const int BPB = 8, BlockSize = 4;

        /// <inheritdoc/>
        public int BlockWidth => BlockSize;

        /// <inheritdoc/>
        public int BlockHeight => BlockSize;

        /// <inheritdoc/>
        public int BytesPerBlock => BPB;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride)
        {
            byte c0 = (byte)((sbyte)source[0] + 128); // min
            byte c1 = (byte)((sbyte)source[1] + 128); // min

            Span<byte> interpolatedColors = stackalloc byte[8];
            BC4UBlock<TColor>.GetInterpolatedColours(c0, c1, interpolatedColors);

            // 6 Bytes = 16 Pixel × 3 Bit
            ulong indices = BinaryPrimitives.ReadUInt64LittleEndian(source) >> 16;

            for (int i = 0; i < 16; i++)
            {
                int idx = (int)(indices & 0b111);
                int row = i / 4;
                int col = i % 4;
                target[row * stride + col].I = interpolatedColors[idx];
                indices >>= 3;
            }
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride)
        {
            (byte c0, byte c1) = BC4UBlock<TColor>.GetMinMax(source, stride);

            Span<byte> palette = stackalloc byte[8];
            BC4UBlock<TColor>.GetInterpolatedColours(c0, c1, palette);

            ulong indices = 0;
            int shift = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    byte v = source[row * stride + col].I;
                    int bestIndex = BC4UBlock<TColor>.GetBestIndex(v, palette);

                    indices |= (ulong)bestIndex << shift;
                    shift += 3;
                }
            }

            BinaryPrimitives.WriteUInt64LittleEndian(target, indices << 16);
            target[0] = (byte)(sbyte)(c0 - 128);
            target[1] = (byte)(sbyte)(c1 - 128);
        }
    }
}
