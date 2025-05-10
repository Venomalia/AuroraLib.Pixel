using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// A structure representing BGA colors in the 5-5-5 format.
    /// </summary>
    public struct BGR555 : IRGB<byte>, IColor<BGR555>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 5, 0, 5, 5, 5, 10);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        // 0BBBBBGG GGGRRRRR
        private ushort PackedValue;

        /// <inheritdoc/>
        public byte R
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue & 0b11111);
            set => PackedValue = PackRGB(value, G, B);
        }

        /// <inheritdoc/>
        public byte G
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue >> 5 & 0b11111);
            set => PackedValue = PackRGB(R, value, B);
        }

        /// <inheritdoc/>
        public byte B
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue >> 10 & 0b11111);
            set => PackedValue = PackRGB(R, G, value);
        }

        readonly byte IRGB<byte>.A => byte.MaxValue;

        private static ushort PackRGB(byte r, byte g, byte b)
            => (ushort)(r >> 3 | (g & 0b1111_1000) << 2 | (b & 0b1111_1000) << 7);

        public BGR555(byte r, byte g, byte b) => PackedValue = PackRGB(r, g, b);

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
        public readonly bool Equals(BGR555 other) => (PackedValue & 0b0111111111111111) == (other.PackedValue & 0b0111111111111111);

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is BGR555 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => PackedValue.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(BGR555)}({B >> 3}, {G >> 3}, {R >> 3})";

        public static implicit operator ushort(BGR555 pixel) => pixel.PackedValue;

        public static implicit operator BGR555(ushort value) => Unsafe.As<ushort, BGR555>(ref value);

    }
}
