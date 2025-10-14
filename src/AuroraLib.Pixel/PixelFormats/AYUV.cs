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
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 8, 16, 8, 8, 8, 0, 8, 24, false, PixelFormatInfo.ChannelFormatType.YUV);
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

        /// <inheritdoc/>
        public bool Equals(AYUV other) => Packed == other.Packed;

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
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= byteMax;
            float r = vector.X;
            float g = vector.Y;
            float b = vector.Z;
            float y = 16 + (0.257f * r + 0.504f * g + 0.098f * b);
            float u = 128 - (0.148f * r + 0.291f * g - 0.439f * b);
            float v = 128 + (0.439f * r - 0.368f * g - 0.071f * b);

            Vector4 yuva = Vector4.Clamp(new Vector4(y, u, v, vector.W) + new Vector4(0.5f), Vector4.Zero, byteMax);
            Y = (byte)yuva.X;
            U = (byte)yuva.Y;
            V = (byte)yuva.Z;
            A = (byte)yuva.W;
        }
        private static readonly Vector4 byteMax = new Vector4(255);

        /// <inheritdoc/>
        public Vector4 ToScaledVector4()
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/dd206750.aspx
            float C = (Y - 16f) * 1.1644f;
            float D = U - 128f;
            float E = V - 128f;

            float r = C + 1.5960f * E;
            float g = C - 0.3917f * D - 0.8128f * E;
            float b = C + 2.0172f * D;

            // [0..1]
            return Vector4.Clamp(new Vector4(r, g, b, A) / 255f, Vector4.Zero, Vector4.One);
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(AYUV)}({Y}, {U}, {V}, {A})";

        private readonly uint Packed => Unsafe.As<AYUV, uint>(ref Unsafe.AsRef(in this));

        public static implicit operator uint(AYUV pixel) => pixel.Packed;
        public static implicit operator AYUV(uint value) => Unsafe.As<uint, AYUV>(ref value);
    }
}
