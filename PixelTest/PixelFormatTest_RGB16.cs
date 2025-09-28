using AuroraLib.Pixel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PixelTest
{
    [TestClass]
    public class PixelFormatTest_RGB16
    {
        public static IEnumerable<object[]> GetAvailablePixelFormats()
        {
            IEnumerable<Type> availableAlgorithmTypes = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(x => x.GetExportedTypes().Where(s => typeof(IRGB<ushort>).IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract));
            return availableAlgorithmTypes.Select(x => new object[] { (IColor)Activator.CreateInstance(x)! });
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void FromScaledVector4(IRGB<ushort> pixel)
        {
            bool hasAlpha = typeof(IRGBA<ushort>).IsAssignableFrom(pixel.GetType());
            var max = ushort.MaxValue;
            var min = ushort.MinValue;
            ushort zeroAlpha = hasAlpha ? ushort.MinValue : ushort.MaxValue;
            // All 1
            pixel.FromScaledVector4(Vector4.One);
            Assert.AreEqual(max, pixel.R);
            Assert.AreEqual(max, pixel.G);
            Assert.AreEqual(max, pixel.B);
            Assert.AreEqual(max, pixel.A);
            // All 0
            pixel.FromScaledVector4(Vector4.Zero);
            Assert.AreEqual(min, pixel.R);
            Assert.AreEqual(min, pixel.G);
            Assert.AreEqual(min, pixel.B);
            Assert.AreEqual(zeroAlpha, pixel.A);
            // All 0 R 1
            pixel.FromScaledVector4(Vector4.UnitX);
            Assert.AreEqual(max, pixel.R);
            Assert.AreEqual(min, pixel.G);
            Assert.AreEqual(min, pixel.B);
            Assert.AreEqual(zeroAlpha, pixel.A);
            // All 0 G 1
            pixel.FromScaledVector4(Vector4.UnitY);
            Assert.AreEqual(min, pixel.R);
            Assert.AreEqual(max, pixel.G);
            Assert.AreEqual(min, pixel.B);
            Assert.AreEqual(zeroAlpha, pixel.A);
            // All 0 B 1
            pixel.FromScaledVector4(Vector4.UnitZ);
            Assert.AreEqual(min, pixel.R);
            Assert.AreEqual(min, pixel.G);
            Assert.AreEqual(max, pixel.B);
            Assert.AreEqual(zeroAlpha, pixel.A);
            // All 0 A 1
            pixel.FromScaledVector4(Vector4.UnitW);
            Assert.AreEqual(min, pixel.R);
            Assert.AreEqual(min, pixel.G);
            Assert.AreEqual(min, pixel.B);
            Assert.AreEqual(max, pixel.A);
        }

        [TestMethod]
        [DynamicData(nameof(GetAvailablePixelFormats), DynamicDataSourceType.Method)]
        public void ToScaledVector4(IRGB<ushort> pixel)
        {
            bool hasAlpha = typeof(IRGBA<ushort>).IsAssignableFrom(pixel.GetType());
            Vector4 zeroAlpha = hasAlpha ? Vector4.Zero : Vector4.UnitW;

            pixel.FromScaledVector4(Vector4.One);
            Assert.AreEqual(Vector4.One, pixel.ToScaledVector4());

            pixel.FromScaledVector4(Vector4.Zero);
            Assert.AreEqual(zeroAlpha, pixel.ToScaledVector4());

            pixel.FromScaledVector4(Vector4.UnitX);
            Assert.AreEqual(Vector4.UnitX + zeroAlpha, pixel.ToScaledVector4());

            pixel.FromScaledVector4(Vector4.UnitY);
            Assert.AreEqual(Vector4.UnitY + zeroAlpha, pixel.ToScaledVector4());

            pixel.FromScaledVector4(Vector4.UnitZ);
            Assert.AreEqual(Vector4.UnitZ + zeroAlpha, pixel.ToScaledVector4());

            pixel.FromScaledVector4(Vector4.UnitW);
            Assert.AreEqual(Vector4.UnitW, pixel.ToScaledVector4());
        }
    }
}
