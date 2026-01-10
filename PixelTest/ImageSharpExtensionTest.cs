using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.ImageSharpExtension;
using AuroraLib.Pixel.PixelFormats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using RGBA32 = AuroraLib.Pixel.PixelFormats.RGBA<byte>;
namespace PixelTest
{
    [TestClass]
    public class ImageSharpExtensionTest
    {
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;

        [TestMethod]
        public void TryAsAuroraImage()
        {
            using var imageSixLabors = new Image<Rgba32>(10, 10);
            Assert.IsTrue(imageSixLabors.TryAsAuroraImage(out var imageAurora));

            if (imageAurora is IImage<RGBA32> aImagergba)
            {
                aImagergba[0, 0] = Black;
                aImagergba[9, 0] = Red;
                aImagergba[0, 9] = Green;
                aImagergba[9, 9] = Blue;
            }
            else
            {
                Assert.Fail();
            }

            Assert.AreEqual(new Rgba32(Black), imageSixLabors[0, 0]);
            Assert.AreEqual(new Rgba32(Red), imageSixLabors[9, 0]);
            Assert.AreEqual(new Rgba32(Green), imageSixLabors[0, 9]);
            Assert.AreEqual(new Rgba32(Blue), imageSixLabors[9, 9]);
        }

        [TestMethod]
        public void CloneAsImageSharp()
        {
            using var imageAurora = new MemoryImage<RGBA32>(10, 10);
            imageAurora[0, 0] = Black;
            imageAurora[9, 0] = Red;
            imageAurora[0, 9] = Green;
            imageAurora[9, 9] = Blue;

            using var imageSixLabors = imageAurora.CloneAsImageSharp<Rgba32>();

            Assert.AreEqual(new Rgba32(Black), imageSixLabors[0, 0]);
            Assert.AreEqual(new Rgba32(Red), imageSixLabors[9, 0]);
            Assert.AreEqual(new Rgba32(Green), imageSixLabors[0, 9]);
            Assert.AreEqual(new Rgba32(Blue), imageSixLabors[9, 9]);
        }
    }
}
