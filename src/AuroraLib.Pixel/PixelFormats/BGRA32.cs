using System.Buffers.Binary;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 32-bit BGRA color, where each component (B, G, R, A) is an 8-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BGRA32 : IRGBA<byte>, IColor<BGRA32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 16, 8, 8, 8, 0, 8, 24);

        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte B { readonly get; set; }
        /// <inheritdoc/>
        public byte G { readonly get; set; }
        /// <inheritdoc/>
        public byte R { readonly get; set; }
        /// <inheritdoc/>
        public byte A { readonly get; set; }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public BGRA32(byte b, byte g, byte r, byte a = byte.MaxValue)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            R = (byte)vector.X;
            G = (byte)vector.Y;
            B = (byte)vector.Z;
            A = (byte)vector.W;
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
            R = value.R;
            G = value.G;
            B = value.B;
            A = value.A;
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = (byte)(value.R >> 8);
            G = (byte)(value.G >> 8);
            B = (byte)(value.B >> 8);
            A = (byte)(value.A >> 8);
        }

        /// <inheritdoc/>
        public readonly bool Equals(BGRA32 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is BGRA32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(BGRA32)}({B}, {G}, {R}, {A})";

        /// <inheritdoc/>
        public static bool operator ==(BGRA32 left, BGRA32 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(BGRA32 left, BGRA32 right) => !(left == right);

        private readonly uint Packed => Unsafe.As<BGRA32, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(BGRA32 pixel) => pixel.Packed;
        public static implicit operator BGRA32(uint value) => Unsafe.As<uint, BGRA32>(ref value);

        public static implicit operator int(BGRA32 pixel) => Unsafe.As<BGRA32, int>(ref pixel);
        public static implicit operator BGRA32(int value) => Unsafe.As<int, BGRA32>(ref value);

        public static implicit operator ARGB32(BGRA32 pixel) => BinaryPrimitives.ReverseEndianness(pixel.Packed);
        public static implicit operator BGRA32(ARGB32 value) => BinaryPrimitives.ReverseEndianness((uint)value);

        public static implicit operator Color(BGRA32 pixel) => Color.FromArgb(pixel);
        public static implicit operator BGRA32(Color value) => value.ToArgb();
    }
}
