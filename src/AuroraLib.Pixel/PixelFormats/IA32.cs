using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a packed 32-bit color format using 16 bits for intensity and 16 bits for alpha.
    /// </summary>
    public struct IA32 : IIntensity<ushort>, IColor<IA32>, IAlpha<ushort>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 16, 0, 16, 0, 16, 0, 16, 16);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;
        /// <inheritdoc/>
        public ushort I { get; set; }

        /// <inheritdoc/>
        public ushort A { get; set; }

        float IIntensity.I
        {
            readonly get => (float)I / ushort.MaxValue;
            set => I = (ushort)(value * ushort.MaxValue);
        }

        float IAlpha.A
        {
            readonly get => (float)A / ushort.MaxValue;
            set => A = (ushort)(value * ushort.MaxValue);
        }

        public IA32(ushort i, ushort a)
        {
            I = i;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(IA32 other) => I == other.I && A == other.A;

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => new Vector4(I, I, I, A) / Help.UshortMaxF;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Help.UshortMaxF;
            I = (ushort)Help.BT709Luminance(vector);
            A = (ushort)vector.W;
        }

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            byte i = (byte)(I >> 8);
            value.R = i;
            value.G = i;
            value.B = i;
            value.A = (byte)(A >> 8);
        }

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
        {
            I = Help.Expand8BitTo16Bit(Help.BT709Luminance8Bit(value));
            A = Help.Expand8BitTo16Bit(value.A);
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
        {
            I = Help.BT709Luminance16Bit(value);
            A = value.A;
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(IA32)}({I}, {A})";
    }
}
