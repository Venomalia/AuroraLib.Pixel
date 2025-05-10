using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 8-bit color format using 4 bits for intensity and 4 bits for alpha.
    /// </summary>
    public struct IA8 : IIntensity<byte>, IColor<IA8>, IAlpha<byte>
    {
        byte PackedValue;

        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(8, 4, 0, 4, 0, 4, 0, 4, 4);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte I
        {
            readonly get => Help.Expand4BitTo8Bit(PackedValue & 0xF);
            set => PackedValue = PackIA4(value, A);
        }

        /// <inheritdoc/>
        public byte A
        {
            readonly get => Help.Expand4BitTo8Bit(PackedValue >> 4);
            set => PackedValue = PackIA4(I, value);
        }

        float IIntensity.I
        {
            readonly get => (float)(PackedValue & 0xF) / 15;
            set => PackedValue = (byte)((PackedValue & 0xF0) | (byte)(value * 15));
        }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        private static byte PackIA4(byte intensity, byte alpha) => (byte)(alpha << 4 | intensity >> 4);

        public IA8(byte i, byte a)
        {
            PackedValue = PackIA4(i, a);
        }

        /// <inheritdoc/>
        public readonly bool Equals(IA8 other) => PackedValue == other.PackedValue;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(I, I, I, A) / Help.ByteMaxF;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            PackedValue = PackIA4((byte)Help.BT709Luminance(vector), (byte)vector.W);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            byte i = I;
            value.R = i;
            value.G = i;
            value.B = i;
            value.A = A;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => PackedValue = PackIA4(Help.BT709Luminance8Bit(value), value.A);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => PackedValue = PackIA4((byte)(Help.BT709Luminance16Bit(value) >> 8), (byte)(value.A >> 8));

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(IA8)}({I>> 4}, {A >> 4})";
    }
}
