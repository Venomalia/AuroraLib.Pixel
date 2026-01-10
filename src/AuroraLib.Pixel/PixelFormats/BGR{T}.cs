using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a generic BGR (Blue, Green, Red) pixel format,
    /// where each color channel is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BGR<TValue> : IRGB<TValue>, IColor<BGR<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public TValue B { readonly get; set; }
        /// <inheritdoc/>
        public TValue G { readonly get; set; }
        /// <inheritdoc/>
        public TValue R { readonly get; set; }

        readonly TValue IRGB<TValue>.A => ScaleHelper<TValue>.MaxValue;

        float IColor.Mask
        {
            readonly get => Help.BT709Luminance(ToScaledVector4());
            set => B = G = R = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        static BGR()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo((byte)(size * 3), size, (byte)(size * 2), size, size, size, 0, 0, 0, PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="BGR{TValue}"/> instance with the specified
        /// red, green and blue channel values.
        /// </summary>
        /// <param name="r">The red channel value.</param>
        /// <param name="g">The green channel value.</param>
        /// <param name="b">The blue channel value.</param>
        public BGR(TValue b, TValue g, TValue r)
        {
            B = b;
            G = g;
            R = r;
        }

        /// <inheritdoc/>
        public readonly bool Equals(BGR<TValue> other)
            => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B);

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is BGR<TValue> rGBA && Equals(rGBA);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            R = ScaleHelper<TValue>.ScaleBits(value.R);
            G = ScaleHelper<TValue>.ScaleBits(value.G);
            B = ScaleHelper<TValue>.ScaleBits(value.B);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = ScaleHelper<byte>.ScaleBits(R);
            value.G = ScaleHelper<byte>.ScaleBits(G);
            value.B = ScaleHelper<byte>.ScaleBits(B);
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            R = ScaleHelper<TValue>.FromScaledFloat(vector.X);
            G = ScaleHelper<TValue>.FromScaledFloat(vector.Y);
            B = ScaleHelper<TValue>.FromScaledFloat(vector.Z);
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => ScaleHelper<TValue>.ToScaledVector4(R, G, B);

        /// <inheritdoc/>
        public override readonly string ToString()
            => $"{nameof(BGR<TValue>)}{Unsafe.SizeOf<TValue>() * 24}{ScaleHelper<TValue>.GetTypeName()}({B}, {G}, {R})";

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(R, G, B);
#else
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + R.GetHashCode();
                hash = hash * 31 + G.GetHashCode();
                hash = hash * 31 + B.GetHashCode();
                return hash;
            }
#endif
        }

        public static bool operator ==(BGR<TValue> left, BGR<TValue> right) => left.Equals(right);
        public static bool operator !=(BGR<TValue> left, BGR<TValue> right) => !(left == right);

        public static implicit operator BGR<TValue>(RGB<TValue> pixel) => new BGR<TValue>(pixel.B, pixel.G, pixel.R);
        public static implicit operator RGB<TValue>(BGR<TValue> pixel) => new RGB<TValue>(pixel.R, pixel.G, pixel.B);
    }
}
