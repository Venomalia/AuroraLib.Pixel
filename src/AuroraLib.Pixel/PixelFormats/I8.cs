using System;
using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents an 8-bit intensity value.
    /// </summary>
    public struct I8 : IIntensity<byte>, IColor<I8>, IIndexColor
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(8, 8, 0, 8, 0, 8, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte I { readonly get; set; }

        int IIndexColor.I
        {
            readonly get => I;
            set => I = (byte)value;
        }
        float IIntensity.I
        {
            readonly get => (float)I / byte.MaxValue;
            set => I = (byte)(value * byte.MaxValue);
        }

        public I8(byte i) => I = i;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => I = (byte)(Help.BT709Luminance(vector) * byte.MaxValue);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(I, I, I, byte.MaxValue) / Help.ByteMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = I;
            value.G = I;
            value.B = I;
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => I = Help.BT709Luminance8Bit(value);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => I = (byte)(Help.BT709Luminance16Bit(value) >> 8);

        /// <inheritdoc/>
        public readonly bool Equals(I8 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is I8 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(I8)}({I})";

        /// <inheritdoc/>
        public static bool operator ==(I8 left, I8 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(I8 left, I8 right) => !(left == right);

        public static implicit operator byte(I8 pixel) => pixel.I;

        public static implicit operator I8(byte b) => new I8(b);
    }
}