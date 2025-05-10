using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents an 32-bit intensity value.
    /// </summary>
    public struct I32 : IIntensity<uint>, IColor<I32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 32, 0, 32, 0, 32, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public uint I { readonly get; set; }

        float IIntensity.I
        {
            readonly get => (float)I / uint.MaxValue;
            set => I = (uint)(value * uint.MaxValue);
        }

        public I32(uint i) => I = i;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => I = (uint)((double)Help.BT709Luminance(vector) * uint.MaxValue);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            float i = (float)((double)I / uint.MaxValue);
            return new Vector4(i, i, i, 1);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            byte i = (byte)(I >> 24);
            value.R = i;
            value.G = i;
            value.B = i;
            value.A = byte.MaxValue;
        }


        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => I = Help.Expand8BitTo32Bit(Help.BT709Luminance8Bit(value));

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => I = Help.Expand16BitTo32Bit(Help.BT709Luminance16Bit(value));

        /// <inheritdoc/>
        public readonly bool Equals(I32 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is I32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(I32)}({I})";

        /// <inheritdoc/>
        public static bool operator ==(I32 left, I32 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(I32 left, I32 right) => !(left == right);

        public static implicit operator uint(I32 pixel) => pixel.I;

        public static implicit operator I32(uint b) => new I32(b);
    }
}