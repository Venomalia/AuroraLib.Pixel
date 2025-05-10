using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 16-bit color in RGBA4444 format, where each component (R, G, B, A) is an 4-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBA16 : IRGBA<byte>, IColor<RGBA16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 4, 12, 4, 8, 4, 4, 4, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        //RRRRGGGG BBBBAAAA
        private byte BA;
        private byte RG;

        /// <inheritdoc/>
        public byte R
        {
            readonly get => Help.Expand4BitTo8Bit(RG >> 4);
            set => RG = (byte)(value >> 4 << 4 | RG & 0b1111);
        }

        /// <inheritdoc/>
        public byte G
        {
            readonly get => Help.Expand4BitTo8Bit(RG & 0b1111);
            set => RG = (byte)(RG & 0xF0 | value >> 4);
        }

        /// <inheritdoc/>
        public byte B
        {
            readonly get => Help.Expand4BitTo8Bit(BA >> 4);
            set => BA = (byte)(value >> 4 << 4 | BA & 0b1111);
        }

        /// <inheritdoc/>
        public byte A
        {
            readonly get => Help.Expand4BitTo8Bit(BA & 0b1111);
            set => BA = (byte)(BA & 0xF0 | value >> 4);
        }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public RGBA16(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            RG = PackByte(r, g);
            BA = PackByte(b, a);
        }
        private static byte PackByte(byte high, byte low) => (byte)(high & 0xF0 | low >> 4);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            RG = PackByte((byte)vector.X, (byte)vector.Y);
            BA = PackByte((byte)vector.Z, (byte)vector.W);
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
            RG = PackByte(value.R, value.G);
            BA = PackByte(value.B, value.A);
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            RG = PackByte((byte)(value.R >> 8), (byte)(value.G >> 8));
            BA = PackByte((byte)(value.B >> 8), (byte)(value.A >> 8));
        }

        /// <inheritdoc/>
        public readonly bool Equals(RGBA16 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is RGBA16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBA16)}({R >> 4}, {G >> 4}, {B >> 4}, {A >> 4})";

        /// <inheritdoc/>
        public static bool operator ==(RGBA16 left, RGBA16 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RGBA16 left, RGBA16 right) => !(left == right);

        private readonly ushort Packed => Unsafe.As<RGBA16, ushort>(ref Unsafe.AsRef(in this));

        public static implicit operator ushort(RGBA16 pixel) => pixel.Packed;

        public static implicit operator RGBA16(ushort value) => Unsafe.As<ushort, RGBA16>(ref value);

        /// <summary>
        /// Parses a 3 or 4 digit hexadecimal RGBA color code into an <see cref="RGBA16"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <param name="result">The parsed <see cref="RGBA16"/> value if successful, otherwise the default.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
#if NET5_0_OR_GREATER
        public static bool TryParseHex(ReadOnlySpan<char> hex, out RGBA16 result)
        {
            result = default;
            if (hex.IsEmpty)
                return false;

            if (hex[0] == '#')
                hex = hex[1..];
#else
        public static bool TryParseHex(string? hex, out RGBA16 result)
        {
            result = default;
            if (string.IsNullOrEmpty(hex))
                return false;

            if (hex![0] == '#')
                hex = hex.Substring(1);
#endif
            if (hex.Length < 1 || hex.Length > 4)
                return false;

            if (ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort packedValue))
            {
                int shift = (4 - hex.Length) * 4;
                result = (ushort)(packedValue << shift);

                if (hex.Length <= 3)
                    result.A = byte.MaxValue;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses a 3 or 4 digit hexadecimal RGBA color code into an <see cref="RGBA16"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <returns>The parsed <see cref="RGBA16"/> color value if successful.</returns>
#if NET5_0_OR_GREATER
        public static RGBA16 ParseHex(ReadOnlySpan<char> hex)
#else
        public static RGBA16 ParseHex(string hex)
#endif
        {
            if (TryParseHex(hex, out RGBA16 rgba))
                return rgba;

            throw new ArgumentException($"Invalid hex format for {nameof(RGBA16)}.", nameof(hex));
        }
    }
}
