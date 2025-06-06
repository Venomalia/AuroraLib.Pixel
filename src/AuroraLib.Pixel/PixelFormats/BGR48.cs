﻿using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 48-bit BGR color, where each component (B, G, R) is an 16-bit value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BGR48 : IRGB<ushort>, IColor<BGR48>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(48, 16, 32, 16, 16, 16, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public ushort B { readonly get; set; }
        /// <inheritdoc/>
        public ushort G { readonly get; set; }
        /// <inheritdoc/>
        public ushort R { readonly get; set; }

        readonly ushort IRGB<ushort>.A => ushort.MaxValue;

        public BGR48(ushort r, ushort g, ushort b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.UshortMaxF;
            R = (ushort)vector.X;
            G = (ushort)vector.Y;
            B = (ushort)vector.Z;
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, ushort.MaxValue) / Help.UshortMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = (byte)(R >> 8);
            value.G = (byte)(G >> 8);
            value.B = (byte)(B >> 8);
            value.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            R = Help.Expand8BitTo16Bit(value.R);
            G = Help.Expand8BitTo16Bit(value.G);
            B = Help.Expand8BitTo16Bit(value.B);
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            R = value.R;
            G = value.G;
            B = value.B;
        }

        /// <inheritdoc/>
        public readonly bool Equals(BGR48 other) => R == other.R && G == other.G && B == other.B;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is BGR48 other && Equals(other);

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(BGR48)}({B}, {G}, {R})";
    }
}
