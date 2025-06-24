using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats
{
    internal static class Help
    {
        public static readonly float ByteMaxF = byte.MaxValue;
        public static readonly float UshortMaxF = ushort.MaxValue;

        public static byte Expand2BitTo8Bit(int value) => (byte)((value & 0b11) * 85);
        public static byte Expand3BitTo8Bit(int value) => (byte)(value << 5 | value << 2 | value >> 1);
        public static byte Expand4BitTo8Bit(int value) => (byte)(value << 4 | value);
        public static byte Expand5BitTo8Bit(int value) => (byte)(value << 3 | value >> 2);
        public static byte Expand6BitTo8Bit(int value) => (byte)(value << 2 | value >> 4);
        public static ushort Expand8BitTo16Bit(byte value) => (ushort)(value << 8 | value);
        public static ushort Expand10BitTo16Bit(ushort value) => (ushort)((value << 6) | (value >> 4));
        public static uint Expand8BitTo32Bit(byte value) => (uint)(value << 24 | value << 16 | value << 8 | value);
        public static uint Expand16BitTo32Bit(ushort value) => (uint)(value << 16 | value);


        private static readonly Vector3 Bt709 = new Vector3(0.2126f, 0.7152f, 0.0722f);

        public static float BT709Luminance(in Vector3 vector)
            => Vector3.Dot(vector, Bt709);

        public static float BT709Luminance(in Vector4 color)
            => BT709Luminance(new Vector3(color.X, color.Y, color.Z));

        public static byte BT709Luminance8Bit<TColor>(TColor color) where TColor : IRGB<byte>
            => (byte)(BT709Luminance(new Vector3(color.R, color.G, color.B)) + 0.5f);

        public static ushort BT709Luminance16Bit<TColor>(TColor color) where TColor : IRGB<ushort>
            => (ushort)(BT709Luminance(new Vector3(color.R, color.G, color.B)) + 0.5f);

    }
}
