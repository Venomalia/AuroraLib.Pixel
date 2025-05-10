using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents an 16-bit intensity value.
    /// </summary>
    public struct I16 : IIntensity<ushort>, IColor<I16>, IIndexColor
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 16, 0, 16, 0, 16, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public ushort I { readonly get; set; }

        int IIndexColor.I
        {
            readonly get => I;
            set => I = (ushort)value;
        }

        float IIntensity.I
        {
            readonly get => (float)I / ushort.MaxValue;
            set => I = (ushort)(value * ushort.MaxValue);
        }

        public I16(ushort i) => I = i;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => I = (ushort)(Help.BT709Luminance(vector) * ushort.MaxValue);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(I, I, I, ushort.MaxValue) / Help.UshortMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            byte i = (byte)(I >> 8);
            value.R = i;
            value.G = i;
            value.B = i;
            value.A = byte.MaxValue;
        }


        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => I = Help.Expand8BitTo16Bit(Help.BT709Luminance8Bit(value));

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => I = Help.BT709Luminance16Bit(value);

        /// <inheritdoc/>
        public readonly bool Equals(I16 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is I16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(I16)}({I})";

        /// <inheritdoc/>
        public static bool operator ==(I16 left, I16 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(I16 left, I16 right) => !(left == right);

        public static implicit operator ushort(I16 pixel) => pixel.I;

        public static implicit operator I16(ushort b) => new I16(b);
    }
}