using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 16-bit color in ARGB4444 format, where each component (A, R, G, B) is an 4-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ARGB16 : IRGBA<byte>, IColor<ARGB16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 4, 8, 4, 4, 4, 0, 4, 12);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        //AAAARRRR GGGGBBBB
        private byte GB;
        private byte AR;

        /// <inheritdoc/>
        public byte A
        {
            readonly get => Help.Expand4BitTo8Bit(AR >> 4);
            set => AR = PackByte(value, R);
        }

        /// <inheritdoc/>
        public byte R
        {
            readonly get => Help.Expand4BitTo8Bit(AR & 0b1111);
            set => AR = PackByte(A, value);
        }

        /// <inheritdoc/>
        public byte G
        {
            readonly get => Help.Expand4BitTo8Bit(GB >> 4);
            set => GB = PackByte(value, B);
        }

        /// <inheritdoc/>
        public byte B
        {
            readonly get => Help.Expand4BitTo8Bit(GB & 0b1111);
            set => GB = PackByte(G, value);
        }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public ARGB16(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            AR = PackByte(a, r);
            GB = PackByte(g, b);
        }

        private static byte PackByte(byte high, byte low) => (byte)(high & 0xF0 | low >> 4);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            AR = PackByte((byte)vector.W, (byte)vector.X);
            GB = PackByte((byte)vector.Y, (byte)vector.Z);
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A) / Help.ByteMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = R;
            value.G = G;
            value.B = B;
            value.A = A;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            AR = PackByte(value.A, value.R);
            GB = PackByte(value.G, value.B);
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            AR = PackByte((byte)(value.A >> 8), (byte)(value.R >> 8));
            GB = PackByte((byte)(value.G >> 8), (byte)(value.B >> 8));
        }

        /// <inheritdoc/>
        public readonly bool Equals(ARGB16 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is ARGB16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(ARGB16)}({A >> 4}, {R >> 4}, {G >> 4}, {B >> 4})";

        /// <inheritdoc/>
        public static bool operator ==(ARGB16 left, ARGB16 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ARGB16 left, ARGB16 right) => !(left == right);

        private readonly ushort Packed => Unsafe.As<ARGB16, ushort>(ref Unsafe.AsRef(in this));

        public static implicit operator ushort(ARGB16 pixel) => pixel.Packed;

        public static implicit operator ARGB16(ushort value) => Unsafe.As<ushort, ARGB16>(ref value);
    }
}
