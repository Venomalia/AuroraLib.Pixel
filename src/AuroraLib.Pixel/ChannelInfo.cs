using System;

namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents information about a color channel, including its bit mask, shift position, and bit depth.
    /// </summary>
    public readonly struct ChannelInfo
    {
        /// <summary>
        /// Bit mask representing this channel.
        /// </summary>
        public readonly ulong Mask;

        /// <summary>
        /// Bit position (LSB) where this channel starts.
        /// </summary>
        public readonly byte Shift;

        /// <summary>
        /// Bit depth of this color channel.
        /// </summary>
        public readonly byte BitDepth;

        public ChannelInfo(byte bitDepth, byte shift)
        {
            if (bitDepth > 64 || shift > 63 || bitDepth + shift > 64)
                throw new ArgumentOutOfRangeException("bitDepth and shift must be within [0, 64) and not overflow.");

            BitDepth = bitDepth;
            Shift = shift;
            Mask = (1UL << bitDepth) - 1 << shift;
        }

        public ChannelInfo(ulong mask)
        {
            if (mask == 0)
            {
                Mask = Shift = BitDepth = 0;
                return;
            }

            Mask = mask;
            Shift = CalculateShift(mask);
            BitDepth = CountBits(mask >> Shift);
        }

        private static byte CalculateShift(ulong mask)
        {
            byte shift = 0;
            while ((mask & 1UL) == 0 && shift < 64)
            {
                mask >>= 1;
                shift++;
            }
            return shift;
        }

        private static byte CountBits(ulong value)
        {
            byte count = 0;
            while (value != 0)
            {
                count += (byte)(value & 1);
                value >>= 1;
            }
            return count;
        }
    }
}
