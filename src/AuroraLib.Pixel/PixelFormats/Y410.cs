using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Y410 pixel format with 10-bit luminance (Y), full chrominance (U and V) and 2-bit alpha per pixel (YUV 4:4:4).
    /// </summary>
    public struct Y410 : IYUV<ushort>, IAlpha<byte>, IColor<Y410>
    {
        const ushort Mask10 = 0b11_1111_1111;

        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 10, 10, 10, 0, 10, 20, 2, 30, PixelFormatInfo.ColorSpaceType.YUV);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        private uint Packed;

        /// <inheritdoc/>
        public ushort U
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((Packed >> 0) & Mask10));
            set => Packed = PackUYVA(value, Y, V, A);
        }
        /// <inheritdoc/>
        public ushort Y
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((Packed >> 10) & Mask10));
            set => Packed = PackUYVA(U, value, V, A);
        }
        /// <inheritdoc/>
        public ushort V
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((Packed >> 20) & Mask10));
            set => Packed = PackUYVA(U, Y, value, A);
        }
        /// <inheritdoc/>
        public byte A
        {
            readonly get => Help.Expand2BitTo8Bit((byte)((Packed >> 30) & 0b11));
            set => Packed = PackUYVA(U, Y, V, value);
        }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        public Y410(ushort u, ushort y, ushort v, byte a = byte.MaxValue) => Packed = PackUYVA(u, y, v, a);

        internal static uint PackUYVA(ushort u, ushort y, ushort v, byte a)
            => PackNativUYVA((ushort)(u >> 6), (ushort)(y >> 6), (ushort)(v >> 6), (byte)(a >> 6));

        internal static uint PackNativUYVA(ushort u, ushort y, ushort v, byte a)
            => (uint)((u & Mask10) | (y & Mask10) << 10 | (v & Mask10) << 20 | (a & 0b11) << 30);

        /// <inheritdoc/>
        public readonly bool Equals(Y410 other) => other.Packed == Packed;

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
            // Clamp input RGBA to [0,1]
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            float r = vector.X;
            float g = vector.Y;
            float b = vector.Z;

            // RGB → YUV (BT.601, normalized [0..1] for RGB)
            float y = 0.299f * r + 0.587f * g + 0.114f * b;
            float u = (b - y) / 1.772f;
            float v = (r - y) / 1.402f;

            // Scale to 10-bit
            float y10 = y * (940f - 64f) + 64f + 0.5f;
            float u10 = u * 448f + 512f + 0.5f;
            float v10 = v * 448f + 512f + 0.5f;
            float a2 = vector.W * 3f + 0.5f;

            Packed = PackNativUYVA((ushort)u10, (ushort)y10, (ushort)v10, (byte)a2);
        }

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
        {
            uint u10 = (Packed >> 0) & 0x03FF;
            uint y10 = (Packed >> 10) & 0x03FF;
            uint v10 = (Packed >> 20) & 0x03FF;
            uint a2 = (Packed >> 30) & 0x03;

            // Normalize YUVA to float
            float y = (y10 - 64f) / (940f - 64f);   // [0..1]
            float u = (u10 - 512f) / 448f;          // [-1..1]
            float v = (v10 - 512f) / 448f;          // [-1..1]
            float a = (float)a2 / 3;

            // BT.601 YUV → RGB
            float r = y + 1.402f * v;
            float g = y - 0.344136f * u - 0.714136f * v;
            float b = y + 1.772f * u;

            return Vector4.Clamp(new Vector4(r, g, b, a), Vector4.Zero, Vector4.One);
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"{nameof(Y410)}({Y}, {U}, {V}, {A})";

        /// <inheritdoc/>
        public override readonly int GetHashCode() => Packed.GetHashCode();

        public static implicit operator uint(Y410 pixel) => pixel.Packed;
        public static implicit operator Y410(uint value) => Unsafe.As<uint, Y410>(ref value);
    }
}
