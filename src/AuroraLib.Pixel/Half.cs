#if NETSTANDARD || NET20_OR_GREATER
using System;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel
{

    public readonly struct Half : IEquatable<Half>, IComparable<Half>, IFormattable
    {
        private readonly ushort _value;

        public Half(ushort value) => _value = value;

        public static explicit operator Half(float f)
        {
            uint bits = Unsafe.As<float, uint>(ref f);
            ushort sign = (ushort)((bits >> 16) & 0x8000);
            uint exp = (bits >> 23) & 0xFF;
            uint mant = bits & 0x7FFFFF;

            if (exp == 0xFF) // NaN or Inf
                return new Half((ushort)(sign | (mant != 0 ? 0x7E00 : 0x7C00)));

            if (exp < 113) // Too small, becomes zero
                return new Half(sign);

            if (exp > 142) // too large, becomes Inf
                return new Half((ushort)(sign | 0x7C00));

            // Adjust bias: float(127) → half(15)
            ushort halfExp = (ushort)((exp - 112) << 10);
            ushort halfMant = (ushort)((mant + 0x1000) >> 13); // round

            return new Half((ushort)(sign | halfExp | (halfMant & 0x03FF)));
        }

        public static implicit operator float(Half h)
        {
            ushort s = h._value;
            uint value;

            if ((s & 0x7FFF) == 0)// Zero (+0 / -0)
            {
                value = ((uint)s) << 16;
            }
            else if ((s & 0x7C00) == 0x7C00)// Inf or NaN
            {
                value = ((uint)(s & 0x8000) << 16) | 0x7F800000 | ((uint)(s & 0x03FF) << 13);
            }
            else if ((s & 0x7C00) == 0) // Subnormal: normalize
            {
                int mant = s & 0x03FF;
                int exp = -1;
                do
                {
                    exp++;
                    mant <<= 1;
                } while ((mant & 0x400) == 0);

                mant &= 0x3FF;
                value = ((uint)(s & 0x8000) << 16) | (uint)((127 - 15 - exp) << 23) | (uint)(mant << 13);
            }
            else // Normal
            {
                value = ((uint)(s & 0x8000) << 16) | (((uint)(s & 0x7C00) + 0x1C000) << 13) | ((uint)(s & 0x03FF) << 13);
            }

            return Unsafe.As<uint, float>(ref value);
        }


        public bool Equals(Half other) => _value == other._value;
        public int CompareTo(Half other) => ((float)this).CompareTo(other);
        public string ToString(string format, IFormatProvider formatProvider) => ((float)this).ToString(format, formatProvider);
    }
}
#endif
