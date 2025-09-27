using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.PixelFormats
{
    /// <summary>
    /// Represents a pixel in BGRA 10-10-10-2 format.
    /// This format stores 10 bits per channel for Blue, Green, and Red, and 2 bits for Alpha, packed into a 32-bit integer.
    /// </summary>
    public struct BGRA1010102 : IRGBA<ushort>, IAlpha<byte>, IColor<BGRA1010102>
    {
        private static Vector4 Multiplier = new Vector4(0b1111111111, 0b1111111111, 0b1111111111, 0b11);

        // AABB_BBBB BBBB_GGGG GGGG_GGRR RRRR_RRRR
        private uint PackedValue;

        /// <inheritdoc cref="IColor.FormatInfo"/>
        public static readonly PixelFormatInfo FormatInfo = new PixelFormatInfo(32, 10, 20, 10, 10, 10, 0, 2, 30);
        readonly PixelFormatInfo IColor.FormatInfo => FormatInfo;

        /// <inheritdoc/>
        public ushort R
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((PackedValue >> 20) & 0b1111111111));
            set => PackedValue = PackRGBA(value, G, B, A);
        }
        /// <inheritdoc/>
        public ushort G
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((PackedValue >> 10) & 0b1111111111));
            set => PackedValue = PackRGBA(R, value, B, A);
        }
        /// <inheritdoc/>
        public ushort B
        {
            readonly get => Help.Expand10BitTo16Bit((ushort)((PackedValue) & 0b1111111111));
            set => PackedValue = PackRGBA(R, G, value, A);
        }
        /// <inheritdoc/>
        public byte A
        {
            readonly get => Help.Expand2BitTo8Bit((int)(PackedValue >> 30));
            set => PackedValue = PackRGBA(R, G, B, value);
        }

        float IColor.Mask
        {
            readonly get => (float)A / byte.MaxValue;
            set => A = (byte)(value * byte.MaxValue);
        }

        ushort IRGBA<ushort>.A { readonly get => Help.Expand8BitTo16Bit(A); set => A = (byte)(value >> 8); }
        ushort IAlpha<ushort>.A { readonly get => Help.Expand8BitTo16Bit(A); set => A = (byte)(value >> 8); }
        readonly ushort IRGB<ushort>.A => Help.Expand8BitTo16Bit(A);


        private static uint PackRGBA(ushort R, ushort G, ushort B, byte A)
            => PackNativRGBA(R >> 6, G >> 6, B >> 6, A >> 6);

        private static uint PackNativRGBA(int R, int G, int B, int A)
            => (uint)B | ((uint)(G) << 10) | ((uint)(R) << 20) | ((uint)(A) << 30);

        /// <inheritdoc/>
        public Vector4 ToScaledVector4() => new Vector4((PackedValue >> 20) & 0b1111111111, (PackedValue >> 10) & 0b1111111111, PackedValue & 0b1111111111, (PackedValue >> 30) & 0b11) / Multiplier;

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
        {
            vector *= Multiplier;
            PackedValue = PackNativRGBA((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
        }

        /// <inheritdoc/>
        public readonly bool Equals(BGRA1010102 other) => PackedValue == other.PackedValue;

        /// <inheritdoc/>
        public readonly void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
        {
            value.R = (byte)(R >> 8);
            value.G = (byte)(G >> 8);
            value.B = (byte)(B >> 8);
            value.A = A;
        }

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => PackedValue = PackRGBA(value.R, value.G, value.B, (byte)(value.A >> 8));

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => PackedValue = PackRGBA(Help.Expand8BitTo16Bit(value.R), Help.Expand8BitTo16Bit(value.G), Help.Expand8BitTo16Bit(value.B), value.A);

        public static implicit operator uint(BGRA1010102 pixel) => pixel.PackedValue;
        public static implicit operator BGRA1010102(uint value) => Unsafe.As<uint, BGRA1010102>(ref value);
    }
}
