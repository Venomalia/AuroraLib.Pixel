using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Also known as RGBA8888, represents a 32-bit RGBA color, where each component (R, G, B, A) is an 8-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBA32 : IRGBA<byte>, IColor<RGBA32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 0, 8, 8, 8, 16, 8, 24);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte R { readonly get; set; }
        /// <inheritdoc/>
        public byte G { readonly get; set; }
        /// <inheritdoc/>
        public byte B { readonly get; set; }
        /// <inheritdoc/>
        public byte A { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public RGBA32(byte r, byte g, byte b, byte a = byte.MaxValue)
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
        public readonly bool Equals(RGBA32 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is RGBA32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBA32)}({R}, {G}, {B}, {A})";

        /// <inheritdoc/>
        public static bool operator ==(RGBA32 left, RGBA32 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(RGBA32 left, RGBA32 right) => !(left == right);

        private readonly uint Packed => Unsafe.As<RGBA32, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(RGBA32 pixel) => pixel.Packed;
        public static implicit operator RGBA32(uint value) => Unsafe.As<uint, RGBA32>(ref value);

        public static implicit operator ABGR32(RGBA32 pixel) => BinaryPrimitives.ReverseEndianness(pixel.Packed);
        public static implicit operator RGBA32(ABGR32 value) => BinaryPrimitives.ReverseEndianness((uint)value);

        /// <summary>
        /// Parses a 3,4,6 or 8 digit hexadecimal RGBA color code into an <see cref="RGBA32"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <param name="result">The parsed <see cref="RGBA32"/> value if successful, otherwise the default.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
#if NET5_0_OR_GREATER
        public static bool TryParseHex(ReadOnlySpan<char> hex, out RGBA32 result)
#else
        public static bool TryParseHex(string? hex, out RGBA32 result)
#endif
        {
            if (ABGR32.TryParseHex(hex, out ABGR32 abgr))
            {
                result = abgr;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Parses a 3,4,6 or 8 digit hexadecimal RGBA color code into an <see cref="RGBA32"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <returns>The parsed <see cref="RGBA32"/> color value if successful.</returns>
#if NET5_0_OR_GREATER
        public static RGBA32 ParseHex(ReadOnlySpan<char> hex)
#else
        public static RGBA32 ParseHex(string hex)
#endif
        {
            if (TryParseHex(hex, out RGBA32 rgba))
                return rgba;

            throw new ArgumentException($"Invalid hex format for {nameof(RGBA32)}.", nameof(hex));
        }
    }
}
