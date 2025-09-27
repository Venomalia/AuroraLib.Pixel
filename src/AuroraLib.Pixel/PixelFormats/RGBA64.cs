using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 64-bit RGBA color, where each component (R, G, B, A) is an 16-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBA64 : IRGBA<ushort>, IColor<RGBA64>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(64, 16, 0, 16, 16, 16, 32, 16, 48);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public ushort R { readonly get; set; }
        /// <inheritdoc/>
        public ushort G { readonly get; set; }
        /// <inheritdoc/>
        public ushort B { readonly get; set; }
        /// <inheritdoc/>
        public ushort A { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / ushort.MaxValue;
            set => A = (ushort)(value * ushort.MaxValue);
        }

        public RGBA64(ushort r, ushort g, ushort b, ushort a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.UshortMaxF;
            R = (ushort)vector.X;
            G = (ushort)vector.Y;
            B = (ushort)vector.Z;
            A = (ushort)vector.W;
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, A) / Help.UshortMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = (byte)(R >> 8);
            value.G = (byte)(G >> 8);
            value.B = (byte)(B >> 8);
            value.A = (byte)(A >> 8);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            R = Help.Expand8BitTo16Bit(value.R);
            G = Help.Expand8BitTo16Bit(value.G);
            B = Help.Expand8BitTo16Bit(value.B);
            A = Help.Expand8BitTo16Bit(value.A);
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = value.R;
            G = value.G;
            B = value.B;
            A = value.A;
        }

        /// <inheritdoc/>
        public readonly bool Equals(RGBA64 other) => Packed == other.Packed;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is RGBA64 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => Packed.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBA64)}({R}, {G}, {B}, {A})";

        private readonly ulong Packed => Unsafe.As<RGBA64, ulong>(ref Unsafe.AsRef(in this));
        public static implicit operator ulong(RGBA64 pixel) => pixel.Packed;
        public static implicit operator RGBA64(ulong value) => Unsafe.As<ulong, RGBA64>(ref value);
    }
}
