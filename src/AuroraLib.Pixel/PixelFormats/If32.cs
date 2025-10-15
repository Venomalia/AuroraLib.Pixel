using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 32-bit grayscale/intensity color using a single 32-bit floating-point channel.
    /// </summary>
    public struct If32 : IIntensity<float>, IColor<If32>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 32, 0, 32, 0, 32, 0, 0, 0, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public float I { readonly get; set; }

        float IColor.Mask { readonly get => I; set => I = value; }

        public If32(float i) => I = i;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => I = Help.BT709Luminance(vector);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4(I, I, I, 1f);

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
        public readonly bool Equals(If32 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is If32 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(If32)}({I})";
    }
}