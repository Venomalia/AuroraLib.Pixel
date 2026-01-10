using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using RGBA32 = AuroraLib.Pixel.PixelFormats.RGBA<byte>;

namespace PixelTest
{
    [TestClass]
    public class MemoryImageTest
    {
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;

        [TestMethod]
        public void SetAndGetPixel()
        {
            using var image = new MemoryImage<RGBA32>(10, 10);
            image[0, 0] = Black;
            image[9, 0] = Red;
            image[0, 9] = Green;
            image[9, 9] = Blue;

            Assert.AreEqual(Black, image[0, 0]);
            Assert.AreEqual(Red, image[9, 0]);
            Assert.AreEqual(Green, image[0, 9]);
            Assert.AreEqual(Blue, image[9, 9]);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[10, 10] = Black);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[-1, -1] = Black);
        }

        [TestMethod]
        public void Crop()
        {
            using var image = new MemoryImage<RGBA32>(10, 10);
            image[2, 2] = Black;
            image[6, 2] = Red;
            image[2, 6] = Green;
            image[6, 6] = Blue;

            image.Crop(new Rectangle(2, 2, 5, 5));

            Assert.AreEqual(Black, image[0, 0]);
            Assert.AreEqual(Red, image[4, 0]);
            Assert.AreEqual(Green, image[0, 4]);
            Assert.AreEqual(Blue, image[4, 4]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[5, 5] = Black);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[-1, -1] = Black);
        }

        [TestMethod]
        public void Clear()
        {
            using var image = new MemoryImage<RGBA32>(10, 10);
            image[0, 0] = Black;
            image[9, 0] = Red;
            image[0, 9] = Green;
            image[9, 9] = Blue;

            image.Clear();

            Assert.AreEqual(default, image[0, 0]);
            Assert.AreEqual(default, image[9, 0]);
            Assert.AreEqual(default, image[0, 9]);
            Assert.AreEqual(default, image[9, 9]);
        }

        [TestMethod]
        public void Clone()
        {
            var image = new MemoryImage<RGBA32>(10, 10);
            image[0, 0] = Black;
            image[9, 0] = Red;
            image[0, 9] = Green;
            image[9, 9] = Blue;

            var clone = image.Clone();
            image[1, 0] = Black;

            Assert.AreEqual(clone.Width, image.Width);
            Assert.AreEqual(clone.Height, image.Height);
            Assert.AreEqual(clone[0, 0], image[0, 0]);
            Assert.AreEqual(clone[9, 0], image[9, 0]);
            Assert.AreEqual(clone[0, 9], image[0, 9]);
            Assert.AreEqual(clone[9, 9], image[9, 9]);
            Assert.AreNotEqual(clone[1, 0], image[1, 0]);
        }
    }
}
