using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a 16-bit color in ARGB1555 format: 1-bit alpha and 5 bits each for red, green, and blue.
    /// </summary>
    public struct ARGB1555 : IRGBA<byte>, IAlpha<bool>, IColor<ARGB1555>
    {
        private const ushort AMask = 0b1000000000000000;

        // ARRRRRGG GGGBBBBB
        private ushort PackedValue;

        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 5, 10, 5, 5, 5, 0, 1, 15);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte R
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue >> 10 & 0b11111);
            set => PackedValue = PackRGBA(value, G, B, A);
        }

        /// <inheritdoc/>
        public byte G
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue >> 5 & 0b11111);
            set => PackedValue = PackRGBA(R, value, B, A);
        }

        /// <inheritdoc/>
        public byte B
        {
            readonly get => Help.Expand5BitTo8Bit(PackedValue & 0b11111);
            set => PackedValue = PackRGBA(R, G, value, A);
        }

        /// <inheritdoc/>
        public bool A
        {
            readonly get => (PackedValue & AMask) == AMask;
            set => PackedValue = (ushort)(PackedValue & AMask - 1 | (value ? AMask : 0));
        }

        float IAlpha.A
        {
            readonly get => A ? 1 : 0;
            set => A = value >= 0.5f;
        }

        readonly byte IRGB<byte>.A => A ? byte.MaxValue : byte.MinValue;
        byte IRGBA<byte>.A { readonly get => A ? byte.MaxValue : byte.MinValue; set => A = value > 0x80; }
        byte IAlpha<byte>.A { readonly get => A ? byte.MaxValue : byte.MinValue; set => A = value > 0x80; }

        private static ushort PackRGBA(byte r, byte g, byte b, bool a)
            => (ushort)(b >> 3 | (g & 0b1111_1000) << 2 | (r & 0b1111_1000) << 7 | (a ? AMask : 0));

        public ARGB1555(byte r, byte g, byte b, bool a) => PackedValue = PackRGBA(r, g, b, a);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A ? byte.MaxValue : byte.MinValue) / Help.ByteMaxF;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            PackedValue = PackRGBA((byte)vector.X, (byte)vector.Y, (byte)vector.Z, vector.W > 0x80);
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = R;
            value.G = G;
            value.B = B;
            value.A = A ? byte.MaxValue : byte.MinValue;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => PackedValue = PackRGBA(value.R, value.G, value.B, value.A > 0x80);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => PackedValue = PackRGBA((byte)(value.R >> 8), (byte)(value.G >> 8), (byte)(value.B >> 8), value.A > 0x8000);

        /// <inheritdoc/>
        public readonly bool Equals(ARGB1555 other) => PackedValue == other.PackedValue;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is ARGB1555 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => PackedValue.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(ARGB1555)}({(A ? 1 : 0)}, {R >> 3}, {G >> 3}, {B >> 3})";

        public static implicit operator ushort(ARGB1555 pixel) => pixel.PackedValue;

        public static implicit operator ARGB1555(ushort value) => Unsafe.As<ushort, ARGB1555>(ref value);
    }
}
