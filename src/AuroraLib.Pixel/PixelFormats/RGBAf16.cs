using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// 
    /// </summary>
    public struct RGBAf16 : IRGBA<Half>, IColor<RGBAf16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(64, 16, 0, 16, 16, 16, 32, 16, 48);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public Half R { readonly get; set; }
        /// <inheritdoc/>
        public Half G { readonly get; set; }
        /// <inheritdoc/>
        public Half B { readonly get; set; }
        /// <inheritdoc/>
        public Half A { readonly get; set; }

        float IAlpha.A { readonly get => (float)A; set => A = (Half)value; }

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
        public bool Equals(RGBAf16 other)
            => Unsafe.As<RGBAf16, ulong>(ref this) == Unsafe.As<RGBAf16, ulong>(ref other);


        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBAf16)}({R}, {G}, {B}, {A})";
    }
}
