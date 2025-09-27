using System.Globalization;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 32-bit ABGR color, where each component (A, B, G, R) is an 8-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ABGR32 : IRGBA<byte>, IColor<ABGR32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 24, 8, 16, 8, 8, 8, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte A { readonly get; set; }
        /// <inheritdoc/>
        public byte B { readonly get; set; }
        /// <inheritdoc/>
        public byte G { readonly get; set; }
        /// <inheritdoc/>
        public byte R { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public ABGR32(byte b, byte g, byte r, byte a = byte.MaxValue)
        {
            A = a;
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
        public readonly bool Equals(ABGR32 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is ABGR32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(ABGR32)}({A}, {B}, {G}, {R})";

        /// <inheritdoc/>
        public static bool operator ==(ABGR32 left, ABGR32 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ABGR32 left, ABGR32 right) => !(left == right);

        private readonly uint Packed => Unsafe.As<ABGR32, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(ABGR32 pixel) => pixel.Packed;
        public static implicit operator ABGR32(uint value) => Unsafe.As<uint, ABGR32>(ref value);

        /// <summary>
        /// Parses a 3,4,6 or 8 digit hexadecimal RGBA color code into an <see cref="ABGR32"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <param name="result">The parsed <see cref="ABGR32"/> value if successful, otherwise the default.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
#if NET5_0_OR_GREATER
        public static bool TryParseHex(ReadOnlySpan<char> hex, out ABGR32 result)
        {
            result = default;
            if (hex.IsEmpty)
                return false;

            if (hex[0] == '#')
                hex = hex[1..];
#else
        public static bool TryParseHex(string? hex, out ABGR32 result)
        {
            result = default;
            if (string.IsNullOrEmpty(hex))
                return false;

            if (hex![0] == '#')
                hex = hex.Substring(1);
#endif
            if (hex.Length < 1 || hex.Length > 8)
                return false;

            // Handle short RGBA16 format (3 or 4 digits)
            if (hex.Length <= 4)
            {
                if (RGBA16.TryParseHex(hex, out RGBA16 rgba16))
                {
                    rgba16.ToRGBA(ref result);
                    return true;
                }
                return false;
            }

            // Handle full 32-bit ABGR format (6 or 8 digits)
            if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint packedValue))
            {
                int shift = (8 - hex.Length) * 4;
                result = packedValue << shift;

                if (hex.Length <= 6)
                    result.A = byte.MaxValue;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses a 3,4,6 or 8 digit hexadecimal RGBA color code into an <see cref="ABGR32"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <returns>The parsed <see cref="ABGR32"/> color value if successful.</returns>
#if NET5_0_OR_GREATER
        public static ABGR32 ParseHex(ReadOnlySpan<char> hex)
#else
        public static ABGR32 ParseHex(string hex)
#endif
        {
            if (TryParseHex(hex, out ABGR32 rgba))
                return rgba;

            throw new ArgumentException($"Invalid hex format for {nameof(ABGR32)}.", nameof(hex));
        }
    }
}
