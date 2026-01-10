using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a generic single-channel intensity (grayscale) pixel format,
    /// where the intensity value is stored using the specified <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The underlying numeric value type used for each color channel.</typeparam>
    public struct I<TValue> : IIntensity<TValue>, IColor<I<TValue>>, IIndexColor where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo;
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        private TValue Value { readonly get; set; }
        TValue IIntensity<TValue>.I { readonly get => Value; set => Value = value; }

        int IIndexColor.I
        {
            readonly get
            {
                if (typeof(TValue) == typeof(byte))
                    return (byte)(object)Value;
                else if (typeof(TValue) == typeof(ushort))
                    return (ushort)(object)Value;
                else
                    throw new NotSupportedException();
            }
            set
            {
                if (typeof(TValue) == typeof(byte))
                    Value = (TValue)(object)(byte)value;
                else if (typeof(TValue) == typeof(ushort))
                    Value = (TValue)(object)(ushort)value;
                else
                    throw new NotSupportedException();
            }
        }

        static I()
        {
            byte size = (byte)(Unsafe.SizeOf<TValue>() * 8);
            FormatInfo = new PixelFormatInfo(size, size, 0, size, 0, size, 0, 0, 0, PixelFormatInfo.ColorSpaceType.RGB, ScaleHelper<TValue>.GetChannelType());
        }

        /// <summary>
        /// Initializes a new <see cref="I{TValue}"/> instance with the specified intensity channel values.
        /// </summary>
        /// <param name="i">The intensity channel value.</param>
        public I(TValue i) => Value = i;

        float IColor.Mask
        {
            readonly get => ScaleHelper<TValue>.ToScaledFloat(Value);
            set => Value = ScaleHelper<TValue>.FromScaledFloat(value);
        }

        /// <inheritdoc/>
        public readonly bool Equals(I<TValue> other) => Value.Equals(other.Value);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = value.G = value.B = ScaleHelper<byte>.ScaleBits(Value);
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => Value = ScaleHelper<TValue>.FromScaledFloat(Help.BT709Luminance(vector));

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            float i = ScaleHelper<TValue>.ToScaledFloat(Value);
            return new Vector4(i, i, i, 1f);
        }

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is I<TValue> other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Value.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() =>
            $"{nameof(I<TValue>)}{Unsafe.SizeOf<TValue>() * 8}{ScaleHelper<TValue>.GetTypeName()}({Value})";

        public static bool operator ==(I<TValue> left, I<TValue> right) => left.Equals(right);

        public static bool operator !=(I<TValue> left, I<TValue> right) => !(left == right);

        public static implicit operator TValue(I<TValue> pixel) => pixel.Value;

        public static implicit operator I<TValue>(TValue b) => Unsafe.As<TValue, I<TValue>>(ref b);
    }
}
