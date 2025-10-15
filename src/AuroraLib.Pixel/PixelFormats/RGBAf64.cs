using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 64-bit RGBA color using 16-bit half-precision float channels.
    /// </summary>
    public struct RGBAf64 : IRGBA<Half>, IColor<RGBAf64>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(64, 16, 0, 16, 16, 16, 32, 16, 48, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public Half R { readonly get; set; }
        /// <inheritdoc/>
        public Half G { readonly get; set; }
        /// <inheritdoc/>
        public Half B { readonly get; set; }
        /// <inheritdoc/>
        public Half A { readonly get; set; }

        float IColor.Mask { readonly get => (float)A; set => A = (Half)value; }

        /// <inheritdoc/>
        public Vector4 ToScaledVector4() => new Vector4((float)R, (float)G, (float)B, (float)A);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            R = (Half)vector.X;
            G = (Half)vector.Y;
            B = (Half)vector.Z;
            A = (Half)vector.W;
        }

        /// <inheritdoc/>
        public void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public bool Equals(RGBAf64 other)
            => Unsafe.As<RGBAf64, ulong>(ref this) == Unsafe.As<RGBAf64, ulong>(ref other);


        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBAf64)}({R}, {G}, {B}, {A})";
    }
}
