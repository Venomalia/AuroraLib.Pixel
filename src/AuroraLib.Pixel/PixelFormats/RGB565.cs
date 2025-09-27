using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 16-bit RGB color in RGB565 format (5 bits red, 6 bits green, 5 bits blue).
    /// </summary>
    public struct RGB565 : IRGB<byte>, IColor<RGB565>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 5, 11, 6, 5, 5, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        // RRRRRGGG GGGBBBBB
        private ushort PackedValue;

        /// <inheritdoc/>
        public byte R
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue >> 11 & 0b11111);
            set => PackedValue = PackRGB(value, G, B);
        }

        /// <inheritdoc/>
        public byte G
        {
            readonly get => Help.Expand6BitTo8Bit(PackedValue >> 5 & 0b111111);
            set => PackedValue = PackRGB(R, value, B);
        }

        /// <inheritdoc/>
        public byte B
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue & 0b11111);
            set => PackedValue = PackRGB(R, G, value);
        }

        float IColor.Mask
        {
            readonly get => Help.BT709Luminance(ToScaledVector4());
            set => B = G = R = (byte)(value * byte.MaxValue);
        }

        private static ushort PackRGB(byte r, byte g, byte b)
            => (ushort)((r & 0xF8) << 8 | // 5 Bit R - Bits 11–15
                        (g & 0xFC) << 3 | // 6 Bit G - Bits 5–10
                        b >> 3);          // 5 Bit B - Bits 0–4

        public RGB565(byte r, byte g, byte b)
            => PackedValue = PackRGB(r, g, b);

        readonly byte IRGB<byte>.A => byte.MaxValue;

        /// <inheritdoc/>
        public readonly bool Equals(RGB565 other) => PackedValue == other.PackedValue;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, byte.MaxValue) / Help.ByteMaxF;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            PackedValue = PackRGB((byte)vector.X, (byte)vector.Y, (byte)vector.Z);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = R;
            value.G = G;
            value.B = B;
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => PackedValue = PackRGB(value.R, value.G, value.B);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => PackedValue = PackRGB((byte)(value.R >> 8), (byte)(value.G >> 8), (byte)(value.B >> 8));

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGB565)}({R >> 3}, {G >> 2}, {B >> 3})";

        public static implicit operator ushort(RGB565 pixel) => pixel.PackedValue;

        public static implicit operator RGB565(ushort value) => Unsafe.As<ushort, RGB565>(ref value);
    }
}
