using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 96-bit RGB color using 32-bit floating-point channels.
    /// </summary>
    public struct RGBf96 : IRGB<float>, IColor<RGBf96>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(96, 32, 0, 32, 32, 32, 64, 0, 0, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public float R { readonly get; set; }
        /// <inheritdoc/>
        public float G { readonly get; set; }
        /// <inheritdoc/>
        public float B { readonly get; set; }
        /// <inheritdoc/>
        public readonly float A => 1f;

        float IColor.Mask
        {
            readonly get => Help.BT709Luminance(ToScaledVector4());
            set => B = G = R = value;
        }


        public RGBf96(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(R, G, B, 1f);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            R = vector.X;
            G = vector.Y;
            B = vector.Z;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        /// <inheritdoc/>
        public readonly bool Equals(RGBf96 other)
            => ToScaledVector4() == other.ToScaledVector4();

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(RGBf96)}({R}, {G}, {B})";

    }
}
