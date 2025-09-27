using System.Buffers.Binary;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 32-bit ARGB color, where each component (A, R, G, B) is an 8-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ARGB32 : IRGBA<byte>, IColor<ARGB32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 8, 8, 16, 8, 24, 8, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte A { readonly get; set; }
        /// <inheritdoc/>
        public byte R { readonly get; set; }
        /// <inheritdoc/>
        public byte G { readonly get; set; }
        /// <inheritdoc/>
        public byte B { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public ARGB32(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            R = r;
            G = g;
            B = b;
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
        public readonly bool Equals(ARGB32 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is ARGB32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(ARGB32)}({A}, {R}, {G}, {B})";

        /// <inheritdoc/>
        public static bool operator ==(ARGB32 left, ARGB32 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ARGB32 left, ARGB32 right) => !(left == right);

        private readonly uint Packed => Unsafe.As<ARGB32, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(ARGB32 pixel) => pixel.Packed;
        public static implicit operator ARGB32(uint value) => Unsafe.As<uint, ARGB32>(ref value);

        public static implicit operator int(ARGB32 pixel) => Unsafe.As<ARGB32, int>(ref pixel);
        public static implicit operator ARGB32(int value) => Unsafe.As<int, ARGB32>(ref value);

        public static implicit operator Color(ARGB32 pixel) => Color.FromArgb(BinaryPrimitives.ReverseEndianness((int)pixel));
        public static implicit operator ARGB32(Color value) => BinaryPrimitives.ReverseEndianness(value.ToArgb());
    }
}
