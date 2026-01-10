using AuroraLib.Pixel.Processing;
using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a generic ABGR (Alpha, Blue, Green, Red) pixel format,
    /// where each color channel is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public partial struct ABGR<TValue> : IRGBA<TValue>, IColor<ABGR<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
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
        public TValue B { readonly get; set; }
        /// <inheritdoc/>
        public TValue G { readonly get; set; }
        /// <inheritdoc/>
        public TValue R { readonly get; set; }

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(A);
            set => A = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        static ABGR()
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
        public ABGR(TValue b, TValue g, TValue r, TValue a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(ABGR<TValue> other)
            => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is ABGR<TValue> rGBA && Equals(rGBA);

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


        public static bool operator ==(ABGR<TValue> left, ABGR<TValue> right) => left.Equals(right);
        public static bool operator !=(ABGR<TValue> left, ABGR<TValue> right) => !(left == right);

        public static implicit operator RGBA<TValue>(ABGR<TValue> v) => new RGBA<TValue>(v.R, v.G, v.B, v.A);
        public static implicit operator ABGR<TValue>(RGBA<TValue> v) => new ABGR<TValue>(v.B, v.G, v.R, v.A);

        public static implicit operator BGRA<TValue>(ABGR<TValue> v) => new BGRA<TValue>(v.B, v.G, v.R, v.A);
        public static implicit operator ABGR<TValue>(BGRA<TValue> v) => new ABGR<TValue>(v.B, v.G, v.R, v.A);

        public static implicit operator ARGB<TValue>(ABGR<TValue> v) => new ARGB<TValue>(v.R, v.G, v.B, v.A);
        public static implicit operator ABGR<TValue>(ARGB<TValue> v) => new ABGR<TValue>(v.B, v.G, v.R, v.A);

        public static implicit operator uint(ABGR<TValue> pixel)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<ABGR<TValue>, uint>(ref pixel);

            ABGR<byte> argb32 = default;
            pixel.ToRGBA(ref argb32);
            return argb32;
        }

        public static implicit operator ABGR<TValue>(uint value)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<uint, ABGR<TValue>>(ref value);

            ABGR<TValue> pixel = default;
            pixel.TryParseHex("000");

            pixel.From8Bit((ABGR<byte>)value);
            return pixel;
        }
    }
}
