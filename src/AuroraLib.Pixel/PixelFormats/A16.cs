using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a color structure that stores only the alpha channel (transparency) as an 16-bit value.
    /// </summary>
    public struct A16 : IAlpha<ushort>, IColor<A16>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(16, 0, 0, 0, 0, 0, 0, 16, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public ushort A { readonly get; set; }

        float IAlpha.A
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public A16(ushort a) => A = a;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => A = (ushort)(vector.W * ushort.MaxValue);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(ushort.MinValue, ushort.MinValue, ushort.MinValue, A) / Help.UshortMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = byte.MinValue;
            value.G = byte.MinValue;
            value.B = byte.MinValue;
            value.A = (byte)(A >> 8);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte> =>
            A = Help.Expand8BitTo16Bit(value.A);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => A = value.A;

        /// <inheritdoc/>
        public readonly bool Equals(A16 other) => A == other.A;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is A16 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => A.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(A16)}({A})";

        /// <inheritdoc/>
        public static bool operator ==(A16 left, A16 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(A16 left, A16 right) => !(left == right);

        public static implicit operator ushort(A16 pixel) => pixel.A;

        public static implicit operator A16(ushort b) => new A16(b);
    }
}