using System;
using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{

    /// <summary>
    /// Represents a 32-bit grayscale/intensity color with alpha using 16-bit half-precision floating-point channels.
    /// </summary>
    public struct IAf32 : IIntensity<Half>, IColor<IAf32>, IAlpha<Half>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 16, 0, 16, 0, 16, 0, 16, 16, PixelFormatInfo.ColorSpaceType.RGB, PixelFormatInfo.ChannelType.Float);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;
        /// <inheritdoc/>
        public Half I { get; set; }

        /// <inheritdoc/>
        public Half A { get; set; }

        float IColor.Mask { readonly get => (float)I; set => I = (Half)value; }

        public IAf32(Half i, Half a)
        {
            I = i;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(IAf32 other) => I == other.I && A == other.A;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4((float)I,(float) I,(float) I, (float)A);

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            I = (Half)Help.BT709Luminance(vector);
            A = (Half)vector.W;
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
        public override readonly string ToString() => $"{nameof(IAf32)}({I}, {A})";
    }
}
