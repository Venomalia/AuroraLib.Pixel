using System;
using System.Buffers.Binary;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes BC4 (DXT5) blocks with unsigned data (0–255 range).
    /// Each 4x4 block is compressed to 8 bytes: 2 reference values + 3-bit indices per pixel.
    /// </summary>
    public sealed class BC4UBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IIntensity<byte>
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
            byte c0 = source[0]; // min
            byte c1 = source[1]; // max

            Span<byte> interpolatedColors = stackalloc byte[8];
            GetInterpolatedColours(c0, c1, interpolatedColors);

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
            (byte c0, byte c1) = GetMinMax(source, stride);

            Span<byte> palette = stackalloc byte[8];
            GetInterpolatedColours(c0, c1, palette);

            ulong indices = 0;
            int shift = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    byte v = source[row * stride + col].I;
                    int bestIndex = GetBestIndex(v, palette);

                    indices |= (ulong)bestIndex << shift;
                    shift += 3;
                }
            }

            BinaryPrimitives.WriteUInt64LittleEndian(target, c0 | ((ulong)c1 << 8) | (indices << 16));
        }

        internal static void GetInterpolatedColours(byte left, byte right, Span<byte> colors)
        {
            colors[0] = left;
            colors[1] = right;

            if (left > right)
            {
                colors[2] = (byte)((6 * left + right) / 7);
                colors[3] = (byte)((5 * left + 2 * right) / 7);
                colors[4] = (byte)((4 * left + 3 * right) / 7);
                colors[5] = (byte)((3 * left + 4 * right) / 7);
                colors[6] = (byte)((2 * left + 5 * right) / 7);
                colors[7] = (byte)((left + 6 * right) / 7);
            }
            else
            {
                colors[2] = (byte)((4 * left + right) / 5);
                colors[3] = (byte)((3 * left + 2 * right) / 5);
                colors[4] = (byte)((2 * left + 3 * right) / 5);
                colors[5] = (byte)((left + 4 * right) / 5);
                colors[6] = byte.MinValue;
                colors[7] = byte.MaxValue;
            }
        }

        internal static (byte max, byte min) GetMinMax(ReadOnlySpan<TColor> source, int stride, byte threshold = 16)
        {
            byte min = byte.MaxValue, minThreshold = byte.MaxValue;
            byte max = byte.MinValue, maxThreshold = byte.MinValue;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    byte v = source[row * stride + col].I;

                    // Min/Max
                    if (v < min) min = v;
                    if (v > max) max = v;

                    // Threshold Min/Max
                    if (v > threshold && v < byte.MaxValue - threshold)
                    {
                        if (v < minThreshold) minThreshold = v;
                        if (v > maxThreshold) maxThreshold = v;
                    }
                }
            }

            if (threshold != 0 &&
                min < 5 && max > 250 &&
                minThreshold != byte.MaxValue &&
                maxThreshold != byte.MinValue)
                return (minThreshold, maxThreshold); // Swap min and max

            return (max, min);
        }

        internal static int GetBestIndex(byte value, ReadOnlySpan<byte> palette)
        {
            int bestIndex = 0;
            int bestDist = int.MaxValue;

            for (int i = 0; i < palette.Length; i++)
            {
                int dist = Math.Abs(value - palette[i]);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }
    }
}
