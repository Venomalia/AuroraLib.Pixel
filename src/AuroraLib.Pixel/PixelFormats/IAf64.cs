using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 64-bit grayscale/intensity color with alpha using 32-bit floating-point channels.
    /// </summary>
    public struct IAf64 : IIntensity<float>, IColor<IAf64>, IAlpha<float>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(64, 32, 0, 32, 0, 32, 0, 32, 32, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;
        /// <inheritdoc/>
        public float I { get; set; }

        /// <inheritdoc/>
        public float A { get; set; }

        float IColor.Mask { readonly get => I; set => I = value; }

        public IAf64(float i, float a)
        {
            I = i;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(IAf64 other) => I == other.I && A == other.A;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(I, I, I, A);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            I = Help.BT709Luminance(vector);
            A = vector.W;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(IAf64)}({I}, {A})";
    }
}
