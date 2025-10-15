using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 128-bit RGBA color using 32-bit floating-point channels.
    /// </summary>
    public struct RGBAf128 : IRGBA<float>, IColor<RGBAf128>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(128, 32, 0, 32, 32, 32, 64, 32, 96, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public float R { readonly get; set; }
        /// <inheritdoc/>
        public float G { readonly get; set; }
        /// <inheritdoc/>
        public float B { readonly get; set; }
        /// <inheritdoc/>
        public float A { readonly get; set; }

        float IColor.Mask { readonly get => A; set => A = value; }

        public RGBAf128(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => (Vector4)this;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            R = vector.X;
            G = vector.Y;
            B = vector.Z;
            A = vector.W;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(this);

        /// <inheritdoc/>
        public readonly bool Equals(RGBAf128 other)
            => (Vector4)this == (Vector4)other;

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBAf128)}({R}, {G}, {B}, {A})";

        public static implicit operator RGBAf128(Vector4 value) => Unsafe.As<Vector4, RGBAf128>(ref value);
        public static implicit operator Vector4(RGBAf128 value) => Unsafe.As<RGBAf128, Vector4>(ref value);
    }
}
