using System;
using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a color structure that stores only the alpha channel (transparency) as an 16-bit float value.
    /// </summary>
    public struct Af16 : IAlpha<Half>, IColor<Af16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 0, 0, 0, 0, 0, 0, 16, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public Half A { readonly get; set; }

        float IAlpha.A
        {
            readonly get => (float)A;
            set => A = (Half)(value);
        }

        public Af16(Half a) => A = a;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => A = (Half)vector.W;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(0f, 0f, 0f, (float)A);

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
        public readonly bool Equals(Af16 other) => A.Equals(other.A);

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is Af16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => A.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(Af16)}({A})";
    }
}