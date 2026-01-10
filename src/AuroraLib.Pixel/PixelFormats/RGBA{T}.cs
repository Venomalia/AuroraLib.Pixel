using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a generic RGBA (Red, Green, Blue, Alpha) pixel format,
    /// where each color channel is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public partial struct RGBA<TValue> : IRGBA<TValue>, IColor<RGBA<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public TValue R { readonly get; set; }
        /// <inheritdoc/>
        public TValue G { readonly get; set; }
        /// <inheritdoc/>
        public TValue B { readonly get; set; }
        /// <inheritdoc/>
        public TValue A { readonly get; set; }

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(A);
            set => A = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        static RGBA()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo((byte)(size * 4), size, 0, size, size, size, (byte)(size * 2), size, (byte)(size * 3), PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="RGBA{TValue}"/> instance with the specified
        /// red, green, blue, and alpha channel values.
        /// </summary>
        /// <param name="r">The red channel value.</param>
        /// <param name="g">The green channel value.</param>
        /// <param name="b">The blue channel value.</param>
        /// <param name="a">The alpha channel value.</param>
        public RGBA(TValue r, TValue g, TValue b, TValue a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(RGBA<TValue> other)
            => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is RGBA<TValue> rGBA && Equals(rGBA);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
            A = ScaleHelper<TValue>.ScaleBits(value.A);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
            A = ScaleHelper<TValue>.ScaleBits(value.A);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = ScaleHelper<byte>.ScaleBits(R);
            value.G = ScaleHelper<byte>.ScaleBits(G);
            value.B = ScaleHelper<byte>.ScaleBits(B);
            value.A = ScaleHelper<byte>.ScaleBits(A);
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => (R, G, B, A) = ScaleHelper<TValue>.FromScaledVector4(vector);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => ScaleHelper<TValue>.ToScaledVector4(R, G, B, A);

        /// <inheritdoc/>
        public override readonly string ToString()
            => $"{nameof(RGBA<TValue>)}{Unsafe.SizeOf<TValue>() * 32}{ScaleHelper<TValue>.GetTypeName()}({R}, {G}, {B}, {A})";

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

        public static bool operator ==(RGBA<TValue> left, RGBA<TValue> right) => left.Equals(right);
        public static bool operator !=(RGBA<TValue> left, RGBA<TValue> right) => !(left == right);

        public static implicit operator uint(RGBA<TValue> pixel)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<RGBA<TValue>, uint>(ref pixel);

            RGBA<byte> rgba32 = default;
            pixel.ToRGBA(ref rgba32);
            return rgba32;
        }

        public static implicit operator RGBA<TValue>(uint value)
        {
            if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
                return Unsafe.As<uint, RGBA<TValue>>(ref value);

            RGBA<TValue> pixel = default;
            pixel.From8Bit((RGBA<byte>)value);
            return pixel;
        }
    }
}
