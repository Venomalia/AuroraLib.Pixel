using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a generic ARGB (Alpha, Red, Green, Blue) pixel format,
    /// where each color channel is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public partial struct ARGB<TValue> : IRGBA<TValue>, IColor<ARGB<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public TValue A { readonly get; set; }
        /// <inheritdoc/>
        public TValue R { readonly get; set; }
        /// <inheritdoc/>
        public TValue G { readonly get; set; }
        /// <inheritdoc/>
        public TValue B { readonly get; set; }

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(A);
            set => A = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        static ARGB()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo((byte)(size * 4), size, size, size, (byte)(size * 2), size, (byte)(size * 3), size, 0, PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="ARGB{TValue}"/> instance with the specified
        /// red, green, blue, and alpha channel values.
        /// </summary>
        /// <param name="r">The red channel value.</param>
        /// <param name="g">The green channel value.</param>
        /// <param name="b">The blue channel value.</param>
        /// <param name="a">The alpha channel value.</param>
        public ARGB(TValue r, TValue g, TValue b, TValue a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(ARGB<TValue> other)
            => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is ARGB<TValue> rGBA && Equals(rGBA);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            A = ScaleHelper<TValue>.ScaleBits(value.A);
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            A = ScaleHelper<TValue>.ScaleBits(value.A);
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.A = ScaleHelper<byte>.ScaleBits(A);
            value.R = ScaleHelper<byte>.ScaleBits(R);
            value.G = ScaleHelper<byte>.ScaleBits(G);
            value.B = ScaleHelper<byte>.ScaleBits(B);
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => (R, G, B, A) = ScaleHelper<TValue>.FromScaledVector4(vector);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => ScaleHelper<TValue>.ToScaledVector4(R, G, B, A);

        /// <inheritdoc/>
        public override readonly string ToString()
            => $"{nameof(ARGB<TValue>)}{Unsafe.SizeOf<TValue>() * 32}{ScaleHelper<TValue>.GetTypeName()}({A}, {R}, {G}, {B})";

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(R, G, B, A);
#else
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + R.GetHashCode();
                hash = hash * 31 + G.GetHashCode();
                hash = hash * 31 + B.GetHashCode();
                hash = hash * 31 + A.GetHashCode();
                return hash;
            }
#endif
        }

        public static bool operator ==(ARGB<TValue> left, ARGB<TValue> right) => left.Equals(right);
        public static bool operator !=(ARGB<TValue> left, ARGB<TValue> right) => !(left == right);

        public static implicit operator RGBA<TValue>(ARGB<TValue> v) => new RGBA<TValue>(v.R, v.G, v.B, v.A);
        public static implicit operator ARGB<TValue>(RGBA<TValue> v) => new ARGB<TValue>(v.R, v.G, v.B, v.A);

        public static implicit operator BGRA<TValue>(ARGB<TValue> v) => new BGRA<TValue>(v.B, v.G, v.R, v.A);
        public static implicit operator ARGB<TValue>(BGRA<TValue> v) => new ARGB<TValue>(v.R, v.G, v.B, v.A);

        public static implicit operator Color(ARGB<TValue> pixel) => Color.FromArgb(BinaryPrimitives.ReverseEndianness((int)(uint)pixel));
        public static implicit operator ARGB<TValue>(Color value) => (uint)BinaryPrimitives.ReverseEndianness(value.ToArgb());

        public static implicit operator uint(ARGB<TValue> pixel)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<ARGB<TValue>, uint>(ref pixel);

            ARGB<byte> argb32 = default;
            pixel.ToRGBA(ref argb32);
            return argb32;
        }

        public static implicit operator ARGB<TValue>(uint value)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<uint, ARGB<TValue>>(ref value);

            ARGB<TValue> pixel = default;
            pixel.From8Bit((ARGB<byte>)value);
            return pixel;
        }
    }
}
