using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents an 4-bit intensity value.
    /// </summary>
    public struct I4 : IIntensity<byte>, IColor<I4>, IIndexColor
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(4, 4, 0, 4, 0, 4, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        private byte i4b;

        byte IIntensity<byte>.I
        {
            readonly get => Help.Expand4BitTo8Bit(i4b);
            set => i4b = (byte)(value >> 4);
        }
        float IColor.Mask
        {
            readonly get => (float)i4b / 15;
            set => i4b = (byte)(value * 15);
        }

        /// <inheritdoc/>
        public int I
        {
            readonly get => i4b;
            set
            {
                i4b = (byte)(value & 0b1111);
            }
        }

        public I4(byte i) => i4b = (byte)(i & 0b1111);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => I = (byte)(Help.BT709Luminance(vector) * 15);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            byte i = Help.Expand4BitTo8Bit(i4b);
            return new Vector4(i, i, i, byte.MaxValue) / Help.ByteMaxF;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            byte i = Help.Expand4BitTo8Bit(i4b);
            value.R = i;
            value.G = i;
            value.B = i;
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => I = Help.BT709Luminance8Bit(value) >> 4;

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => I = Help.BT709Luminance16Bit(value) >> 12;

        /// <inheritdoc/>
        public readonly bool Equals(I4 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is I4 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(I4)}({I})";

        /// <inheritdoc/>
        public static bool operator ==(I4 left, I4 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(I4 left, I4 right) => !(left == right);
    }
}