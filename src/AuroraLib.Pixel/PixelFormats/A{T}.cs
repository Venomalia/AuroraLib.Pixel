using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a generic single-channel alpha (transparency) pixel format,
    /// where the alpha value is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public struct A<TValue> : IAlpha<TValue>, IColor<A<TValue>> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        private TValue Value { readonly get; set; }
        TValue IAlpha<TValue>.A { readonly get => Value; set => Value = value; }

        static A()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo(size, 0, 0, 0, 0, 0, 0, size, 0, PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="A{TValue}"/> instance with the specified alpha channel values.
        /// </summary>
        /// <param name="a">The alpha channel value.</param>
        public A(TValue a) => Value = a;

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(Value);
            set => Value = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        /// <inheritdoc/>
        public readonly bool Equals(A<TValue> other) => Value.Equals(other.Value);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => Value = ScaleHelper<TValue>.ScaleBits(value.A);

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => Value = ScaleHelper<TValue>.ScaleBits(value.A);

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = value.G = value.B = byte.MinValue;
            value.A = ScaleHelper<byte>.ScaleBits(Value);
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => Value = ScaleHelper<TValue>.FromScaledFloat(vector.W);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(0, 0, 0, ScaleHelper<TValue>.ToScaledFloat(Value));

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is A<TValue> other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString()=>
            $"{nameof(A<TValue>)}{Unsafe.SizeOf<TValue>() * 8}{ScaleHelper<TValue>.GetTypeName()}({Value})";

        public static bool operator ==(A<TValue> left, A<TValue> right) => left.Equals(right);

        public static bool operator !=(A<TValue> left, A<TValue> right) => !(left == right);

        public static implicit operator TValue(A<TValue> pixel) => pixel.Value;

        public static implicit operator A<TValue>(TValue b) => Unsafe.As<TValue, A<TValue>>(ref b);
    }
}
