using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 16-bit color format using 8 bits for intensity and 8 bits for alpha.
    /// </summary>
    public struct IA16 : IIntensity<byte>, IColor<IA16>, IAlpha<byte>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 8, 0, 8, 0, 8, 0, 8, 8);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte I { get; set; }

        /// <inheritdoc/>
        public byte A { get; set; }

        float IIntensity.I
        {
            readonly get => (float)I / byte.MaxValue;
            set => I = (byte)(value * byte.MaxValue);
        }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public IA16(byte i, byte a)
        {
            I = i;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(IA16 other) => I == other.I && A == other.A;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(I, I, I, A) / Help.ByteMaxF;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            I = (byte)Help.BT709Luminance(vector);
            A = (byte)vector.W;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = I;
            value.G = I;
            value.B = I;
            value.A = A;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            I = Help.BT709Luminance8Bit(value);
            A = value.A;
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            I = (byte)(Help.BT709Luminance16Bit(value) >> 8);
            A = (byte)(value.A >> 8);
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(IA16)}({I}, {A})";
    }
}
