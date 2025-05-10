using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 32-bit BGRA color, where each component (B, G, R, A) is an 8-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BGR24 : IRGB<byte>, IColor<BGR24>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(24, 8, 16, 8, 8, 8, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte B { readonly get; set; }
        /// <inheritdoc/>
        public byte G { readonly get; set; }
        /// <inheritdoc/>
        public byte R { readonly get; set; }

        readonly byte IRGB<byte>.A => byte.MaxValue;

        public BGR24(byte b, byte g, byte r)
        {
            B = b;
            G = g;
            R = r;
        }
        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            R = (byte)vector.X;
            G = (byte)vector.Y;
            B = (byte)vector.Z;
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, byte.MaxValue) / Help.ByteMaxF;

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
        {
            R = value.R;
            G = value.G;
            B = value.B;
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = (byte)(value.R >> 8);
            G = (byte)(value.G >> 8);
            B = (byte)(value.B >> 8);
        }

        /// <inheritdoc/>
        public readonly bool Equals(BGR24 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is BGR24 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(BGR24)}({B}, {G}, {R})";

        private readonly uint Packed => (uint)(R | G << 8 | B << 16);
    }
}
