using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a color structure that stores only the alpha channel (transparency) as an 8-bit value.
    /// </summary>
    public struct A8 : IAlpha<byte>, IColor<A8>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(8, 0, 0, 0, 0, 0, 0, 8, 0);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte A { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public A8(byte a) => A = a;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => A = (byte)(vector.W * byte.MaxValue);

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(byte.MinValue, byte.MinValue, byte.MinValue, A) / Help.ByteMaxF;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = byte.MinValue;
            value.G = byte.MinValue;
            value.B = byte.MinValue;
            value.A = A;
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte> =>
            A = value.A;

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => A = (byte)(value.A >> 8);

        /// <inheritdoc/>
        public readonly bool Equals(A8 other) => A == other.A;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is A8 other && Equals(other);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => A.GetHashCode();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(A8)}({A})";

        /// <inheritdoc/>
        public static bool operator ==(A8 left, A8 right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(A8 left, A8 right) => !(left == right);

        public static implicit operator byte(A8 pixel) => pixel.A;

        public static implicit operator A8(byte b) => new A8(b);
    }
}