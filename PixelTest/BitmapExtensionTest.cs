using AuroraLib.Pixel.BitmapExtension;
using AuroraLib.Pixel.Image;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Imaging;
using BGRA32 = AuroraLib.Pixel.PixelFormats.BGRA<byte>;
using RGBA32 = AuroraLib.Pixel.PixelFormats.RGBA<byte>;

namespace PixelTest
{
    [TestClass]
    public class BitmapExtensionTest
    {
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;

        [TestMethod]
        //[DataRow(PixelFormat.Format1bppIndexed)] // not supported
        //[DataRow(PixelFormat.Format4bppIndexed)] // not supported
        [DataRow(PixelFormat.Format8bppIndexed)]
        //[DataRow(PixelFormat.Format16bppGrayScale)] // Has no GetPixel...
        [DataRow(PixelFormat.Format16bppRgb555)]
        [DataRow(PixelFormat.Format16bppRgb565)]
        [DataRow(PixelFormat.Format16bppArgb1555)]
        [DataRow(PixelFormat.Format24bppRgb)] // May have an unsupported Data.Stride!
        [DataRow(PixelFormat.Format32bppRgb)]
        [DataRow(PixelFormat.Format32bppArgb)]
        [DataRow(PixelFormat.Format32bppPArgb)]
        //[DataRow(PixelFormat.Format48bppRgb)] // not supported, it's not 48 bpp, rather 39 bpp with a gamma of 1!
        //[DataRow(PixelFormat.Format64bppArgb)] // Same here.
        //[DataRow(PixelFormat.Format64bppPArgb)] // Same here.
        public void AsAuroraImage(PixelFormat format)
        {
            using var imageBitmap = new Bitmap(12, 12, format);

            using (IImage imageAurora = imageBitmap.AsAuroraImage())
            {
                imageAurora[0, 0] = Black.ToScaledVector4();
                imageAurora[11, 0] = Red.ToScaledVector4();
                imageAurora[0, 11] = Green.ToScaledVector4();
                imageAurora[11, 11] = Blue.ToScaledVector4();
            }
            Assert.AreEqual(ToRGBA32(Black), (BGRA32)imageBitmap.GetPixel(0, 0));
            Assert.AreEqual(ToRGBA32(Red), (BGRA32)imageBitmap.GetPixel(11, 0));
            Assert.AreEqual(ToRGBA32(Green), (BGRA32)imageBitmap.GetPixel(0, 11));
            Assert.AreEqual(ToRGBA32(Blue), (BGRA32)imageBitmap.GetPixel(11, 11));
        }

        [TestMethod]
        public void CloneAsBitmap()
        {
            using var imageAurora = new MemoryImage<RGBA32>(10, 10);
            imageAurora[0, 0] = Black;
            imageAurora[9, 0] = Red;
            imageAurora[0, 9] = Green;
            imageAurora[9, 9] = Blue;

            using var imageBitmap = imageAurora.CloneAsBitmap();
            Assert.AreEqual(ToRGBA32(Black), (BGRA32)imageBitmap.GetPixel(0, 0));
            Assert.AreEqual(ToRGBA32(Red), (BGRA32)imageBitmap.GetPixel(9, 0));
            Assert.AreEqual(ToRGBA32(Green), (BGRA32)imageBitmap.GetPixel(0, 9));
            Assert.AreEqual(ToRGBA32(Blue), (BGRA32)imageBitmap.GetPixel(9, 9));
        }

        public BGRA32 ToRGBA32(RGBA32 rgba)
        {
            BGRA32 bgra = default;
            bgra.From8Bit(rgba);
            return bgra;
        }
    }
}
