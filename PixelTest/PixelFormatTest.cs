using AuroraLib.Pixel;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.PixelFormats.Wrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PixelTest
{
    [TestClass]
    public class PixelFormatTest
    {
        [DataTestMethod]
        public void CheckSNORM_AreEqual()
        {
            SNORM<RGBA64> pixel = default;
            SNORM<RGBA64> other = default;
            pixel.FromScaledVector4(Vector4.One);
            other.FromScaledVector4(Vector4.One);

            Assert.AreEqual(pixel, other);
        }

        [DataTestMethod]
        public void CheckSNORM_AreNotEqual()
        {
            SNORM<RGBA32> pixel = default;
            SNORM<RGBA32> other = default;
            pixel.FromScaledVector4(Vector4.One);
            other.FromScaledVector4(Vector4.Zero);

            Assert.AreNotEqual(pixel, other);
        }

        [DataTestMethod]
        public void CheckSNORM_Accuracy_sbyte()
        {
            Span<sbyte> native = new sbyte[] { 0 };
            Span<SNORM<I8>> wrapper = MemoryMarshal.Cast<sbyte, SNORM<I8>>(native);

            for (int i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                native[0] = (sbyte)i;
                float fixedVec = wrapper[0].ToScaledVector4().X;
                float correct = ToSNORMFloat(native[0]);

                // Check
                var diff = Math.Abs(fixedVec - correct);
                if (diff > 0.0000001)
                    Assert.Fail();
            }
            static float ToSNORMFloat(sbyte s) => (s + 128f) / 255f;
        }

        [DataTestMethod]
        public void CheckSNORM_Accuracy_short()
        {
            Span<short> native = new short[] { 0 };
            Span<SNORM<I16>> wrapper = MemoryMarshal.Cast<short, SNORM<I16>>(native);

            for (int i = short.MinValue; i < short.MaxValue; i++)
            {
                native[0] = (short)i;
                float fixedVec = wrapper[0].ToScaledVector4().X;
                float correct = ToSNORMFloat(native[0]);

                // Check
                var diff = Math.Abs(fixedVec - correct);
                if (diff > 0.0000001)
                    Assert.Fail();
            }
            static float ToSNORMFloat(short s) => (s + 32768f) / 65535f;
        }

        [DataTestMethod]
        [DataRow("#FFfFFFFF", byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        [DataRow("00000000", byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue)]
        [DataRow("#FF0000", byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue)]
        [DataRow("#00Ff00", byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue)]
        [DataRow("#0000FF", byte.MinValue, byte.MinValue, byte.MaxValue, byte.MaxValue)]
        [DataRow("#12345678", (byte)0x12, (byte)0x34, (byte)0x56, (byte)0x78)]
        [DataRow("#FFFF", byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)]
        [DataRow("#f00", byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue)]
        [DataRow("0F0", byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue)]
        [DataRow("#00F", byte.MinValue, byte.MinValue, byte.MaxValue, byte.MaxValue)]
        [DataRow("000F", byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue)]
        [DataRow("1234", (byte)0x11, (byte)0x22, (byte)0x33, (byte)0x44)]
        public void ParseHex(string hex, byte r, byte g, byte b, byte a)
        {
            Assert.IsTrue(RGBA32.TryParseHex(hex, out RGBA32 result));
            var expected = new RGBA32(r, g, b, a);

            Assert.AreEqual(result, expected);
        }

        [DataTestMethod]
        [DataRow("FFFF#")]
        [DataRow("#ARGB")]
        [DataRow("##FFFFFFFF")]
        [DataRow("#Xyz")]
        [DataRow("000000000000F")]
        public void ParseInvalidHex(string hex)
        {
            Assert.IsFalse(RGBA32.TryParseHex(hex, out _));
            Assert.ThrowsException<ArgumentException>(() => RGBA32.ParseHex(hex));
        }

        public static IEnumerable<object[]> GetAvailablePixelFormats()
        {
            IEnumerable<Type> availableAlgorithmTypes = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(x => x.GetExportedTypes().Where(s => typeof(IColor).IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract && !s.ContainsGenericParameters));
            return availableAlgorithmTypes.Select(x => new object[] { (IColor)Activator.CreateInstance(x)! });
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void AreEqual(IColor pixel)
        {
            IColor other = (IColor)Activator.CreateInstance(pixel.GetType())!;
            pixel.FromScaledVector4(Vector4.One);
            other.FromScaledVector4(Vector4.One);

            Assert.AreEqual(pixel, other);
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void AreNotEqual(IColor pixel)
        {
            IColor other = (IColor)Activator.CreateInstance(pixel.GetType())!;
            pixel.FromScaledVector4(Vector4.One);
            other.FromScaledVector4(Vector4.Zero);
            Assert.AreNotEqual(pixel, other);
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void GetHashCode(IColor pixel)
        {
            IColor other = (IColor)Activator.CreateInstance(pixel.GetType())!;
            pixel.FromScaledVector4(Vector4.One);
            other.FromScaledVector4(Vector4.One);

            Assert.AreEqual(pixel.GetHashCode(), other.GetHashCode());
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void ScaledVector4(IColor pixel)
        {
            bool isGrayscale = IsGrayscale(pixel);
            bool hasAlpha = HasAlpha(pixel);
            bool hasColor = HasColor(pixel);
            Vector4 zeroAlpha = hasAlpha ? Vector4.Zero : Vector4.UnitW;
            Vector4 actual;

            pixel.FromScaledVector4(Vector4.One);
            actual = pixel.ToScaledVector4();
            Assert.AreEqual(hasColor ? Vector4.One : Vector4.UnitW, actual);

            pixel.FromScaledVector4(Vector4.Zero);
            actual = pixel.ToScaledVector4();
            Assert.AreEqual(Vector4.Zero + zeroAlpha, actual);

            if (!isGrayscale)
            {
                pixel.FromScaledVector4(Vector4.UnitX);
                actual = Round(pixel.ToScaledVector4(), 2);
                Assert.AreEqual(Vector4.UnitX + zeroAlpha, actual);

                pixel.FromScaledVector4(Vector4.UnitY);
                actual = Round(pixel.ToScaledVector4(), 2);
                Assert.AreEqual(Vector4.UnitY + zeroAlpha, actual);

                pixel.FromScaledVector4(Vector4.UnitZ);
                actual = Round(pixel.ToScaledVector4(), 2);
                Assert.AreEqual(Vector4.UnitZ + zeroAlpha, actual);
            }
            else if (hasColor)
            {
                pixel.FromScaledVector4(Vector4.UnitX);
                actual = pixel.ToScaledVector4();
                Assert.AreNotEqual(Vector4.One, actual);
                Assert.AreNotEqual(Vector4.Zero, actual);

                pixel.FromScaledVector4(Vector4.UnitY);
                actual = pixel.ToScaledVector4();
                Assert.AreNotEqual(Vector4.One, actual);
                Assert.AreNotEqual(Vector4.Zero, actual);

                pixel.FromScaledVector4(Vector4.UnitZ);
                actual = pixel.ToScaledVector4();
                Assert.AreNotEqual(Vector4.One, actual);
                Assert.AreNotEqual(Vector4.Zero, actual);
            }

            pixel.FromScaledVector4(Vector4.UnitW);
            actual = pixel.ToScaledVector4();
            Assert.AreEqual(Vector4.UnitW, actual);

            Vector4 Round(Vector4 v, int digits)
                => new Vector4(
                    (float)Math.Round((decimal)v.X, digits),
                    (float)Math.Round((decimal)v.Y, digits),
                    (float)Math.Round((decimal)v.Z, digits),
                    (float)Math.Round((decimal)v.W, digits)
);
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void ToRGBA(IColor pixel)
        {
            bool isGrayscale = IsGrayscale(pixel);
            bool hasAlpha = HasAlpha(pixel);
            bool hasColor = HasColor(pixel);
            byte zeroAlpha = hasAlpha ? byte.MinValue : byte.MaxValue;
            byte maxColor = hasColor ? byte.MaxValue : byte.MinValue;
            RGBA32 actual = new RGBA32();

            pixel.FromScaledVector4(Vector4.One);
            pixel.ToRGBA(ref actual);
            Assert.AreEqual(new RGBA32(maxColor, maxColor, maxColor, byte.MaxValue), actual);

            pixel.FromScaledVector4(Vector4.Zero);
            pixel.ToRGBA(ref actual);
            Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, zeroAlpha), actual);

            if (!isGrayscale)
            {
                pixel.FromScaledVector4(Vector4.UnitX);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MaxValue, byte.MinValue, byte.MinValue, zeroAlpha), actual);

                pixel.FromScaledVector4(Vector4.UnitY);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MaxValue, byte.MinValue, zeroAlpha), actual);

                pixel.FromScaledVector4(Vector4.UnitZ);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MaxValue, zeroAlpha), actual);

                pixel.FromScaledVector4(Vector4.UnitW);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue), actual);
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void From8Bit(IColor pixel)
        {
            bool isGrayscale = IsGrayscale(pixel);
            bool hasAlpha = HasAlpha(pixel);
            bool hasColor = HasColor(pixel);
            byte zeroAlpha = hasAlpha ? byte.MinValue : byte.MaxValue;
            byte maxColor = hasColor ? byte.MaxValue : byte.MinValue;
            RGBA32 pixelRGBA = new RGBA32();

            pixelRGBA.FromScaledVector4(Vector4.One);
            pixel.From8Bit(pixelRGBA);
            pixel.ToRGBA(ref pixelRGBA);
            Assert.AreEqual(new RGBA32(maxColor, maxColor, maxColor, byte.MaxValue), pixelRGBA);


            pixelRGBA.FromScaledVector4(Vector4.Zero);
            pixel.From8Bit(pixelRGBA);
            pixel.ToRGBA(ref pixelRGBA);
            Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, zeroAlpha), pixelRGBA);

            if (!isGrayscale)
            {
                pixelRGBA.FromScaledVector4(Vector4.UnitX);
                pixel.From8Bit(pixelRGBA);
                pixel.ToRGBA(ref pixelRGBA);
                Assert.AreEqual(new RGBA32(byte.MaxValue, byte.MinValue, byte.MinValue, zeroAlpha), pixelRGBA);

                pixelRGBA.FromScaledVector4(Vector4.UnitY);
                pixel.From8Bit(pixelRGBA);
                pixel.ToRGBA(ref pixelRGBA);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MaxValue, byte.MinValue, zeroAlpha), pixelRGBA);

                pixelRGBA.FromScaledVector4(Vector4.UnitZ);
                pixel.From8Bit(pixelRGBA);
                pixel.ToRGBA(ref pixelRGBA);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MaxValue, zeroAlpha), pixelRGBA);

                pixelRGBA.FromScaledVector4(Vector4.UnitW);
                pixel.From8Bit(pixelRGBA);
                pixel.ToRGBA(ref pixelRGBA);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue), pixelRGBA);
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void From16Bit(IColor pixel)
        {
            bool isGrayscale = IsGrayscale(pixel);
            bool hasAlpha = HasAlpha(pixel);
            bool hasColor = HasColor(pixel);
            byte zeroAlpha = hasAlpha ? byte.MinValue : byte.MaxValue;
            byte maxColor = hasColor ? byte.MaxValue : byte.MinValue;
            RGBA32 actual = new RGBA32();
            RGBA64 pixelRGBA64 = new RGBA64();

            pixelRGBA64.FromScaledVector4(Vector4.One);
            pixel.From16Bit(pixelRGBA64);
            pixel.ToRGBA(ref actual);
            Assert.AreEqual(new RGBA32(maxColor, maxColor, maxColor, byte.MaxValue), actual);


            pixelRGBA64.FromScaledVector4(Vector4.Zero);
            pixel.From16Bit(pixelRGBA64);
            pixel.ToRGBA(ref actual);
            Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, zeroAlpha), actual);

            if (!isGrayscale)
            {
                pixelRGBA64.FromScaledVector4(Vector4.UnitX);
                pixel.From16Bit(pixelRGBA64);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MaxValue, byte.MinValue, byte.MinValue, zeroAlpha), actual);

                pixelRGBA64.FromScaledVector4(Vector4.UnitY);
                pixel.From16Bit(pixelRGBA64);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MaxValue, byte.MinValue, zeroAlpha), actual);

                pixelRGBA64.FromScaledVector4(Vector4.UnitZ);
                pixel.From16Bit(pixelRGBA64);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MaxValue, zeroAlpha), actual);

                pixelRGBA64.FromScaledVector4(Vector4.UnitW);
                pixel.From16Bit(pixelRGBA64);
                pixel.ToRGBA(ref actual);
                Assert.AreEqual(new RGBA32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue), actual);
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void FormatInfo(IColor pixel)
        {
            var formatInfo = pixel.FormatInfo;
            bool hasAlpha = HasAlpha(pixel);
            Assert.AreEqual(hasAlpha, formatInfo.HasAlpha);

            bool hasColor = HasColor(pixel);
            Assert.AreEqual(hasColor, formatInfo.HasColor);

            bool isGrayscale = IsGrayscale(pixel);
            Assert.AreEqual(isGrayscale, formatInfo.IsGrayscale);

            int size = Marshal.SizeOf(pixel.GetType());
            Assert.AreEqual(size, (formatInfo.BitsPerPixel + 7) / 8);

            bool isFloat = IsFloat(pixel);
            Assert.AreEqual(isFloat, formatInfo.Type == PixelFormatInfo.ChannelType.Float);


            bool isYUV = IsYUV(pixel);
            Assert.AreEqual(isYUV, formatInfo.ColorSpace == PixelFormatInfo.ColorSpaceType.YUV);

            if (formatInfo.Type == PixelFormatInfo.ChannelType.Dynamic || formatInfo.Type == PixelFormatInfo.ChannelType.Float)
                return;

            if (isGrayscale)
            {
                var redChannelInfo = formatInfo.RedChannelInfo;
                ChannelInfo(pixel, redChannelInfo, Vector4.One, (uint)((ulong)1 << redChannelInfo.BitDepth) - 1);
                ChannelInfo(pixel, redChannelInfo, Vector4.Zero, 0);
            }
            else if (formatInfo.ColorSpace == PixelFormatInfo.ColorSpaceType.RGB)
            {
                var redChannelInfo = formatInfo.RedChannelInfo;
                ChannelInfo(pixel, redChannelInfo, Vector4.UnitX, (uint)(1 << redChannelInfo.BitDepth) - 1);
                ChannelInfo(pixel, redChannelInfo, Vector4.One - Vector4.UnitX, 0);

                var greenChannelInfo = formatInfo.GreenChannelInfo;
                ChannelInfo(pixel, greenChannelInfo, Vector4.UnitY, (uint)(1 << greenChannelInfo.BitDepth) - 1);
                ChannelInfo(pixel, greenChannelInfo, Vector4.One - Vector4.UnitY, 0);

                var blueChannelInfo = formatInfo.BlueChannelInfo;
                ChannelInfo(pixel, blueChannelInfo, Vector4.UnitZ, (uint)(1 << blueChannelInfo.BitDepth) - 1);
                ChannelInfo(pixel, blueChannelInfo, Vector4.One - Vector4.UnitZ, 0);
            }

            if (hasAlpha)
            {
                var alphaChannelInfo = formatInfo.AlphaChannelInfo;
                ChannelInfo(pixel, alphaChannelInfo, Vector4.UnitW, (uint)(1 << alphaChannelInfo.BitDepth) - 1);
                ChannelInfo(pixel, alphaChannelInfo, Vector4.One - Vector4.UnitW, 0);
            }

            void ChannelInfo(IColor pixel, ChannelInfo channelInfo, Vector4 initially, uint expected)
            {
                pixel.FromScaledVector4(initially);
                ulong data = GetDataFromIColor(pixel);
                uint actual = (uint)((data & channelInfo.Mask) >> channelInfo.Shift);
                Assert.AreEqual(expected, actual);
            }
        }

        private static bool IsFloat(IColor pixel)
            => typeof(IAlpha<Half>).IsAssignableFrom(pixel.GetType()) || typeof(IAlpha<float>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<Half>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<float>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<float>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<Half>).IsAssignableFrom(pixel.GetType());

        private static bool HasAlpha(IColor pixel)
            => typeof(IAlpha<byte>).IsAssignableFrom(pixel.GetType()) || typeof(IAlpha<ushort>).IsAssignableFrom(pixel.GetType()) || typeof(IAlpha<uint>).IsAssignableFrom(pixel.GetType()) || typeof(IAlpha<float>).IsAssignableFrom(pixel.GetType()) || typeof(IAlpha<Half>).IsAssignableFrom(pixel.GetType());

        private static bool IsIntensity(IColor pixel)
            => typeof(IIntensity<byte>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<ushort>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<uint>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<float>).IsAssignableFrom(pixel.GetType()) || typeof(IIntensity<Half>).IsAssignableFrom(pixel.GetType());

        private static bool IsRGB(IColor pixel)
            => typeof(IRGB<byte>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<ushort>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<uint>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<float>).IsAssignableFrom(pixel.GetType()) || typeof(IRGB<Half>).IsAssignableFrom(pixel.GetType());

        private static bool IsYUV(IColor pixel)
            => typeof(IYUV<byte>).IsAssignableFrom(pixel.GetType()) || typeof(IYUV<ushort>).IsAssignableFrom(pixel.GetType()) || typeof(IYUV<uint>).IsAssignableFrom(pixel.GetType()) || typeof(IYUV<float>).IsAssignableFrom(pixel.GetType());


        private static bool IsGrayscale(IColor pixel)
            => !(IsRGB(pixel) || IsYUV(pixel));
        private static bool HasColor(IColor pixel)
            => IsRGB(pixel) || IsIntensity(pixel) || IsYUV(pixel);

        private ulong GetDataFromIColor(IColor pixel)
        {
            int size = Marshal.SizeOf(pixel.GetType());
            byte[] bytes = new byte[size];

            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            Marshal.StructureToPtr(pixel, ptr, false);
            return size switch
            {
                1 => bytes[0],
                2 => BitConverter.ToUInt16(bytes, 0),
                3 => (ulong)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16)),
                4 => BitConverter.ToUInt32(bytes, 0),
                6 => (bytes[0] | ((ulong)bytes[1] << 8) | ((ulong)bytes[2] << 16) | ((ulong)bytes[3] << 24) | ((ulong)bytes[4] << 32) | ((ulong)bytes[5] << 40)),
                8 => BitConverter.ToUInt64(bytes, 0),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
