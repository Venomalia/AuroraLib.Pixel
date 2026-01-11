using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// YUV3 pixel format with 8-bit luminance (Y), and full chrominance (U and V) per pixel (YUV 4:4:4).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct YUV3 : IYUV<byte>, IColor<YUV3>
    {
        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(24, 8, 16, 8, 8, 8, 0, 0, 0, PixelFormatInfo.ColorSpaceType.YUV);

        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public byte V { readonly get; set; }
        /// <inheritdoc/>
        public byte U { readonly get; set; }
        /// <inheritdoc/>
        public byte Y { readonly get; set; }

        /// <inheritdoc/>
        public float Mask { readonly get => (float)Y / byte.MaxValue; set => Y = (byte)(value * byte.MaxValue); }

        public YUV3(byte v, byte u, byte y)
        {
            V = v;
            U = u;
            Y = y;
        }

        /// <inheritdoc/>
        public readonly bool Equals(YUV3 other) => other.V == V && other.U == U && other.Y == Y;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is YUV3 yUV && Equals(yUV);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        public void FromScaledVector4(Vector4 vector)
        {
            AYUV ayuv = default;
            ayuv.FromScaledVector4(vector);
            Y = ayuv.Y;
            U = ayuv.U;
            V = ayuv.V;
        }

        /// <inheritdoc/>
        public Vector4 ToScaledVector4() => new AYUV(V, U, Y).ToScaledVector4();

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(YUV3)}({Y}, {U}, {V})";

        /// <inheritdoc/>
        public override readonly int GetHashCode()
#if NET6_0_OR_GREATER
            =>  HashCode.Combine(V, U, Y);
#else
        {
            int hashCode = 458064701;
            hashCode = hashCode * -1521134295 + V.GetHashCode();
            hashCode = hashCode * -1521134295 + U.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
#endif
    }
}
