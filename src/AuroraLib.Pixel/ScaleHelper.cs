using AuroraLib.Pixel.PixelFormats;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel
{
#if DEBUG
    public static class ScaleHelper<TValue> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#else
    internal static class ScaleHelper<TValue> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#endif
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        private static readonly float Scale;

        static ScaleHelper()
        {
            if (typeof(TValue) == typeof(float) || typeof(TValue) == typeof(Half))
            {
                Scale = 1;
                return;
            }

#if NET8_0_OR_GREATER && false
            double MaxValue = double.CreateTruncating(TValue.MaxValue);
            double MinValue = double.CreateTruncating(TValue.MinValue);
            double Value = MaxValue - MinValue;
#else
            double Value;
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
            {
                Value = byte.MaxValue;
            }
            else if (typeof(TValue) == typeof(ushort) || typeof(TValue) == typeof(short))
            {
                Value = ushort.MaxValue;
            }
            else if (typeof(TValue) == typeof(uint) || typeof(TValue) == typeof(int))
            {
                Value = uint.MaxValue;
            }
            else
            {
                throw new NotSupportedException();
            }
#endif
            Scale = (float)(1.0 / (Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToScaledFloat(TValue v)
        {
            if (typeof(TValue) == typeof(float))
                return (float)(object)v;
            else if (typeof(TValue) == typeof(Half))
                return (float)(Half)(object)v;
            else if (typeof(TValue) == typeof(byte))
                return (byte)(object)v * Scale;
            else if (typeof(TValue) == typeof(ushort))
                return (ushort)(object)v * Scale;
            else if (typeof(TValue) == typeof(uint))
                return (uint)(object)v * Scale;
            else if (typeof(TValue) == typeof(sbyte))
                return ((float)(sbyte)(object)v - sbyte.MinValue) * Scale;
            else if (typeof(TValue) == typeof(short))
                return ((float)(short)(object)v - short.MinValue) * Scale;
            else if (typeof(TValue) == typeof(int))
                return IntToScaledFloat(v);
            else
                throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float IntToScaledFloat(TValue v)
            => ((long)(int)(object)v - int.MinValue) * Scale;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue IntFromScaledFloat(float v)
            => (TValue)(object)(int)Math.Min(((double)v / Scale) + int.MinValue, int.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue UIntFromScaledFloat(float v)
            => (TValue)(object)(uint)Math.Min((double)v / Scale, uint.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue FromScaledFloat(float v)
        {
            if (typeof(TValue) == typeof(float))
                return (TValue)(object)v;
            else if (typeof(TValue) == typeof(Half))
                return (TValue)(object)(Half)v;

            v = Clamp(v, 0, 1);
            if (typeof(TValue) == typeof(byte))
                return (TValue)(object)(byte)(v * byte.MaxValue);
            else if (typeof(TValue) == typeof(ushort))
                return (TValue)(object)(ushort)(v * ushort.MaxValue);
            else if (typeof(TValue) == typeof(uint))
                return UIntFromScaledFloat(v);
            else if (typeof(TValue) == typeof(sbyte))
                return (TValue)(object)(sbyte)((v * byte.MaxValue) + sbyte.MinValue);
            else if (typeof(TValue) == typeof(short))
                return (TValue)(object)(short)((v * ushort.MaxValue) + short.MinValue);
            else if (typeof(TValue) == typeof(int))
                return IntFromScaledFloat(v);
            else
                throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(float value, float min, float max)
#if !NET6_0_OR_GREATER
        {
            if (value < min) return min;
            else if (value > max) return max;
            return value;
        }
#else
            => Math.Clamp(value, min, max);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToScaledVector4(TValue r, TValue g, TValue b, TValue a)
            => new Vector4(ToScaledFloat(r), ToScaledFloat(g), ToScaledFloat(b), ToScaledFloat(a));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (TValue r, TValue g, TValue b, TValue a) FromScaledVector4(Vector4 v)
            => (FromScaledFloat(v.X), FromScaledFloat(v.Y), FromScaledFloat(v.Z), FromScaledFloat(v.W));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToScaledVector4(TValue r, TValue g, TValue b)
            => new Vector4(ToScaledFloat(r), ToScaledFloat(g), ToScaledFloat(b), 1f);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue ScaleBits<TValueIn>(TValueIn value) where TValueIn : unmanaged, IEquatable<TValueIn>, IComparable<TValueIn>
#if NET8_0_OR_GREATER
                , INumber<TValueIn>, IMinMaxValue<TValueIn>
#endif
        {
            if (typeof(TValueIn) == typeof(TValue))
                return Unsafe.As<TValueIn, TValue>(ref value);
            else if (typeof(TValueIn) == typeof(byte) && typeof(TValue) == typeof(ushort))
                return (TValue)(object)Help.Expand8BitTo16Bit((byte)(object)value);
            else if (typeof(TValueIn) == typeof(ushort) && typeof(TValue) == typeof(byte))
                return (TValue)(object)(byte)((ushort)(object)value >> 8);
#if NET8_0_OR_GREATER
            else if (typeof(TValueIn) == typeof(float) || typeof(TValue) == typeof(float)
             || typeof(TValueIn) == typeof(Half) || typeof(TValue) == typeof(Half))
                return ScaleHelper<TValue>.FromScaledFloat(ScaleHelper<TValueIn>.ToScaledFloat(value));

            int inBits = Unsafe.SizeOf<TValueIn>() * 8;
            int outBits = Unsafe.SizeOf<TValue>() * 8;
            bool signedIn = TValueIn.MinValue < TValueIn.Zero;
            bool signedOut = TValue.MinValue < TValue.Zero;

            long v = long.CreateTruncating(value);

            if (inBits > outBits)
            {
                v >>= (inBits - outBits);
            }
            else if (inBits < outBits)
            {
                int repeatCount = (outBits + inBits - 1) / inBits;
                for (int i = 1; i < repeatCount; i++)
                    v = (v << inBits) | v;
            }

            if (signedIn && !signedOut)
                v += 1L << (outBits - 1);
            else if (!signedIn && signedOut)
                v -= 1L << (outBits - 1);

            return TValue.CreateTruncating(v);
#else
            return ScaleHelper<TValue>.FromScaledFloat(ScaleHelper<TValueIn>.ToScaledFloat(value));
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFloteType()
            => typeof(TValue) == typeof(float) || typeof(TValue) == typeof(Half) || typeof(TValue) == typeof(double);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSignedType()
            => typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(short) || typeof(TValue) == typeof(int) || typeof(TValue) == typeof(long);

        public static PixelFormatInfo.ChannelType GetChannelType()
        {
            if (IsSignedType())
                return PixelFormatInfo.ChannelType.Signed;
            else if (IsFloteType())
                return PixelFormatInfo.ChannelType.Float;
            else
                return PixelFormatInfo.ChannelType.Unsigned;
        }

#if NET8_0_OR_GREATER
        public static TValue MaxValue => ScaleHelper<TValue>.IsFloteType() ? TValue.One : TValue.MaxValue;
#else
        public static readonly TValue MaxValue = ScaleHelper<TValue>.FromScaledFloat(1f);
#endif

        public static char GetTypeName()
        {
            if (IsSignedType())
                return 'S';
            else if (IsFloteType())
                return 'F';
            else
                return 'U';
        }
    }
}
