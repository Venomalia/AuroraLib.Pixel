using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// AYUV pixel format with 8-bit alpha, luminance (Y), and full chrominance (U and V) per pixel (YUV 4:4:4).
    /// </summary>
    public struct AYUV : IYUV<byte>, IAlpha<byte>, IColor<AYUV>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 16, 8, 8, 8, 0, 8, 24, PixelFormatInfo.ColorSpaceType.YUV);

        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte V { readonly get; set; }
        /// <inheritdoc/>
        public byte U { readonly get; set; }
        /// <inheritdoc/>
        public byte Y { readonly get; set; }
        /// <inheritdoc/>
        public byte A { readonly get; set; }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public AYUV(byte v, byte u, byte y, byte a = byte.MaxValue)
        {
            V = v;
            U = u;
            Y = y;
            A = a;
        }

        /// <inheritdoc/>
        public readonly bool Equals(AYUV other) => Packed == other.Packed;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is AYUV aYUV && Equals(aYUV);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        /// <inheritdoc/>
        public void FromScaledVector4g(Vector4 vector)
        {
            vector *= Help.ByteMaxF;
            float r = vector.X;
            float g = vector.Y;
            float b = vector.Z;
            float y = 16 + (0.257f * r + 0.504f * g + 0.098f * b);
            float u = 128 - (0.148f * r + 0.291f * g - 0.439f * b);
            float v = 128 + (0.439f * r - 0.368f * g - 0.071f * b);

            Vector4 yuva = Vector4.Clamp(new Vector4(y, u, v, vector.W) + new Vector4(0.5f), Vector4.Zero, new Vector4(Help.ByteMaxF));
            Y = (byte)yuva.X; 
            U = (byte)yuva.Y;
            V = (byte)yuva.Z;
            A = (byte)yuva.W;
        }

        public void FromScaledVector4(Vector4 vector)
        {
            // Clamp input RGBA to [0,1]
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            float r = vector.X;
            float g = vector.Y;
            float b = vector.Z;

            // RGB → YUV (BT.601, normalized [0..1] for RGB)
            float yf = 0.257f * r + 0.504f * g + 0.098f * b;
            float uf = -0.148f * r - 0.291f * g + 0.439f * b;
            float vf = 0.439f * r - 0.368f * g - 0.071f * b;

            // Scale to 8-bit
            Y = (byte)(yf * 255f + 16f + 0.5f);
            U = (byte)(uf * 255f + 128f + 0.5f);
            V = (byte)(vf * 255f + 128f + 0.5f);
            A = (byte)(vector.W * 255f + 0.5f);
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            // Normalize YUVA to float
            float y = (Y - 16f) / (235f - 16f);   // [0..1]
            float u = (U - 128f) / 112f;          // [-1..1]
            float v = (V - 128f) / 112f;          // [-1..1]
            float a = A / 255f;

            // BT.601 YUV → RGB
            float c = 1.164383f * y;
            float r = c + 1.596027f * v;
            float g = c - 0.391762f * u - 0.812968f * v;
            float b = c + 2.017232f * u;
            return Vector4.Clamp(new Vector4(r, g, b, a), Vector4.Zero, Vector4.One);
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(AYUV)}({Y}, {U}, {V}, {A})";

        /// <inheritdoc/>
        public override readonly int GetHashCode() => Packed.GetHashCode();

        private readonly uint Packed => Unsafe.As<AYUV, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(AYUV pixel) => pixel.Packed;
        public static implicit operator AYUV(uint value) => Unsafe.As<uint, AYUV>(ref value);
    }
}
