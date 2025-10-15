using System;
using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a 16-bit grayscale/intensity color using a 16-bit half-precision floating-point channel.
    /// </summary>
    public struct If16 : IIntensity<Half>, IColor<If16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 16, 0, 16, 0, 16, 0, 0, 0, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public Half I { readonly get; set; }

        float IColor.Mask { readonly get => (float)I; set => I = (Half)value; }

        public If16(Half i) => I = i;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => I = (Half)Help.BT709Luminance(vector);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4() => new Vector4((float)I, (float)I, (float)I, 1f);

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
        public readonly bool Equals(If16 other) => I == other.I;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is If16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => I.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(If16)}({I})";
    }
}