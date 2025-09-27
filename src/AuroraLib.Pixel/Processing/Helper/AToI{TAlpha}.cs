using System.Numerics;

namespace AuroraLib.Pixel.Processing.Helper
{
    internal struct AToI<TAlpha> : IColor<AToI<TAlpha>>, IIntensity<byte> where TAlpha : unmanaged, IColor<TAlpha>, IAlpha<byte>
    {
        private TAlpha color;

        public byte I { readonly get => color.A; set => color.A = value; }
        float IColor.Mask { readonly get => (float)I / byte.MaxValue; set => I = (byte)(value * byte.MaxValue); }

        public PixelFormatInfo FormatInfo => color.FormatInfo;
        public readonly void From16Bit<TColor1>(TColor1 value) where TColor1 : IRGB<ushort> => color.From16Bit(value);
        public readonly void From8Bit<TColor1>(TColor1 value) where TColor1 : IRGB<byte> => color.From8Bit(value);
        public readonly void FromScaledVector4(Vector4 vector) => color.FromScaledVector4(vector);
        public Vector4 ToScaledVector4() => color.ToScaledVector4();
        public void ToRGBA<TColor1>(ref TColor1 value) where TColor1 : IRGBA<byte> => color.ToRGBA(ref value);
        public bool Equals(AToI<TAlpha> other) => color.Equals(other.color);
    }
}
