using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;

namespace PixelTest
{
    [TestClass]
    public class MemoryPaletteImageTest
    {
        private readonly RGBA32 Transparent = uint.MinValue;
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;
        private readonly RGBA32 Yellow = 0xFF00FFFF;
        private readonly RGBA32 Gray = 0xFFAAAAAA;

        [TestMethod]
        public void SetAndGetPixel()
        {
            using var image = new MemoryPaletteImage<I8, RGBA32>(10, 10);
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

            Assert.AreEqual(5, image.ColorsUsed);
        }

        [TestMethod]
        public void Crop()
        {
            using var image = new MemoryPaletteImage<I8, RGBA32>(10, 10);
            image[2, 2] = Black;
            image[6, 2] = Red;
            image[2, 6] = Green;
            image[6, 6] = Blue;

            image[0, 0] = Yellow; // Will be removed from the Palette.

            image.Crop(new Rectangle(2, 2, 5, 5));

            Assert.AreEqual(Black, image[0, 0]);
            Assert.AreEqual(Red, image[4, 0]);
            Assert.AreEqual(Green, image[0, 4]);
            Assert.AreEqual(Blue, image[4, 4]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[5, 5] = Black);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => image[-1, -1] = Black);
            Assert.AreEqual(5, image.ColorsUsed);
        }

        [TestMethod]
        public void Clear()
        {
            using var image = new MemoryPaletteImage<I8, RGBA32>(10, 10);
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
            using var image = new MemoryPaletteImage<I8, RGBA32>(10, 10);
            image[0, 0] = Black;
            image[9, 0] = Red;
            image[0, 9] = Green;
            image[9, 9] = Blue;

            var clone = (IPaletteImage<RGBA32>)image.Clone();
            image[1, 0] = Black;

            Assert.AreEqual(clone.Width, image.Width);
            Assert.AreEqual(clone.Height, image.Height);
            Assert.AreEqual(clone[0, 0], image[0, 0]);
            Assert.AreEqual(clone[9, 0], image[9, 0]);
            Assert.AreEqual(clone[0, 9], image[0, 9]);
            Assert.AreEqual(clone[9, 9], image[9, 9]);
            Assert.AreNotEqual(clone[1, 0], image[1, 0]);
            Assert.AreEqual(clone.ColorsUsed, image.ColorsUsed);
        }

        [TestMethod]
        public void PaletteColorQuantization()
        {
            using var image = new MemoryPaletteImage<I4, RGBA32>(10, 10, 0, 4);

            // Assign different colors to four corners
            image[0, 0] = Black; // Expected to map to color index 1
            image[9, 0] = Gray;  // Expected to be combined with Black (quantized or deduplicated)
            image[0, 9] = Green; // Expected to map to color index 3
            image[9, 9] = Blue;  // Expected to map to color index 2


            // Check palette entries
            Assert.AreEqual(image.Palette[0], Transparent);
            Assert.AreNotEqual(image.Palette[1], Black);
            Assert.AreEqual(image.Palette[2], Blue);
            Assert.AreEqual(image.Palette[3], Green);

            // Ensure pixels resolve to expected palette colors
            RGBA32 darkGray = image.Palette[1];
            Assert.AreEqual(darkGray, image[0, 0]);
            Assert.AreEqual(darkGray, image[9, 0]);
            Assert.AreEqual(Green, image[0, 9]);
            Assert.AreEqual(Blue, image[9, 9]);

            // Verify number of colors used in palette (including transparent)
            Assert.AreEqual(4, image.ColorsUsed);
        }
    }
}
