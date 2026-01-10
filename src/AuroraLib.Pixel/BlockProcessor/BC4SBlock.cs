using System;
using System.Buffers.Binary;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Encodes and decodes BC4 (DXT5) blocks with signed normalized data (-1.0 to 1.0 range, stored as SNORM).
    /// Each 4x4 block is compressed to 8 bytes: 2 reference values + 3-bit indices per pixel.
    /// </summary>
    public sealed class BC4SBlock<TColor> : IBlockProcessor<TColor> where TColor : unmanaged, IColor<TColor>, IIntensity<sbyte>
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
            sbyte c0 = (sbyte)source[0]; // min
            sbyte c1 = (sbyte)source[1]; // min

            Span<sbyte> interpolatedColors = stackalloc sbyte[8];
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
            (sbyte c0, sbyte c1) = GetMinMax(source, stride);

            Span<sbyte> palette = stackalloc sbyte[8];
            GetInterpolatedColours(c0, c1, palette);

            ulong indices = 0;
            int shift = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    sbyte v = source[row * stride + col].I;
                    int bestIndex = GetBestIndex(v, palette);

                    indices |= (ulong)bestIndex << shift;
                    shift += 3;
                }
            }

            BinaryPrimitives.WriteUInt64LittleEndian(target, indices << 16);
            target[0] = (byte)(sbyte)(c0 - 128);
            target[1] = (byte)(sbyte)(c1 - 128);
        }

        private static void GetInterpolatedColours(sbyte left, sbyte right, Span<sbyte> colors)
        {
            colors[0] = left;
            colors[1] = right;

            if (left > right)
            {
                colors[2] = (sbyte)((6 * left + right) / 7);
                colors[3] = (sbyte)((5 * left + 2 * right) / 7);
                colors[4] = (sbyte)((4 * left + 3 * right) / 7);
                colors[5] = (sbyte)((3 * left + 4 * right) / 7);
                colors[6] = (sbyte)((2 * left + 5 * right) / 7);
                colors[7] = (sbyte)((left + 6 * right) / 7);
            }
            else
            {
                colors[2] = (sbyte)((4 * left + right) / 5);
                colors[3] = (sbyte)((3 * left + 2 * right) / 5);
                colors[4] = (sbyte)((2 * left + 3 * right) / 5);
                colors[5] = (sbyte)((left + 4 * right) / 5);
                colors[6] = 0;
                colors[7] = sbyte.MaxValue;
            }
        }

        internal static (sbyte max, sbyte min) GetMinMax(ReadOnlySpan<TColor> source, int stride, sbyte threshold = 16)
        {
            sbyte min = sbyte.MaxValue, minThreshold = sbyte.MaxValue;
            sbyte max = sbyte.MinValue, maxThreshold = sbyte.MinValue;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    sbyte v = source[row * stride + col].I;

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

            if (threshold != sbyte.MinValue &&
                min < sbyte.MinValue + 5 && max > sbyte.MaxValue - 5 &&
                minThreshold != sbyte.MaxValue &&
                maxThreshold != sbyte.MinValue)
                return (minThreshold, maxThreshold); // Swap min and max

            return (max, min);
        }

        private static int GetBestIndex(sbyte value, ReadOnlySpan<sbyte> palette)
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
