using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a generic IA (Intensity, Alpha) pixel format,
    /// where each channel is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public struct IA<TValue> : IIntensity<TValue>, IAlpha<TValue>, IColor<IA<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public TValue I { readonly get; set; }
        /// <inheritdoc/>
        public TValue A { readonly get; set; }

        static IA()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo((byte)(size * 2), size, 0, size, 0, size, 0, size, size, PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="IA{TValue}"/> instance with the specified intensity and alpha channel values.
        /// </summary>
        /// <param name="i">The intensity channel value.</param>
        /// <param name="a">The alpha channel value.</param>
        public IA(TValue i, TValue a)
        {
            I = i;
            A = a;
        }

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(A);
            set => A = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        /// <inheritdoc/>
        public readonly bool Equals(IA<TValue> other) => I.Equals(other.I) && A.Equals(other.A);

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is IA<TValue> other && Equals(other);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            I = ScaleHelper<TValue>.FromScaledFloat(Help.BT709Luminance(value.R, value.G, value.B) / (float)ushort.MaxValue);
            A = ScaleHelper<TValue>.ScaleBits(value.A);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            I = ScaleHelper<TValue>.FromScaledFloat(Help.BT709Luminance(value.R, value.G, value.B) / (float)byte.MaxValue);
            A = ScaleHelper<TValue>.ScaleBits(value.A);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = value.G = value.B = ScaleHelper<byte>.ScaleBits(I);
            value.A = ScaleHelper<byte>.ScaleBits(A);
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            I = ScaleHelper<TValue>.FromScaledFloat(Help.BT709Luminance(vector));
            A = ScaleHelper<TValue>.FromScaledFloat(vector.W);
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            float g = ScaleHelper<TValue>.ToScaledFloat(I);
            return new Vector4(g, g, g, ScaleHelper<TValue>.ToScaledFloat(A));
        }

        /// <inheritdoc/>
        public readonly override int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(I, A);
#else
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + I.GetHashCode();
                hash = hash * 31 + A.GetHashCode();
                return hash;
            }
#endif
        }

        /// <inheritdoc/>
        public override readonly string ToString()
            => $"{nameof(IA<TValue>)}{Unsafe.SizeOf<TValue>() * 16}{ScaleHelper<TValue>.GetTypeName()}({I}, {A})";

        public static bool operator ==(IA<TValue> left, IA<TValue> right) => left.Equals(right);

        public static bool operator !=(IA<TValue> left, IA<TValue> right) => !(left == right);

    }
}
