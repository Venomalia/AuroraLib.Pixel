using AuroraLib.Pixel.BitmapExtension;
using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.SkiaSharpExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace PixelTest
{
    [TestClass]
    public class SkiaSharpExtensionTest
    {
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;

        [TestMethod]
        //[DataRow(SKColorType.Alpha8)]
        [DataRow(SKColorType.Rgb565)]
        [DataRow(SKColorType.Argb4444)]
        [DataRow(SKColorType.Rgba8888)]
        [DataRow(SKColorType.Rgb888x)]
        [DataRow(SKColorType.Bgra8888)]
        [DataRow(SKColorType.Rgba1010102)]
        [DataRow(SKColorType.Rgb101010x)]
        //[DataRow(SKColorType.Gray8)]
        [DataRow(SKColorType.RgbaF16)]
        [DataRow(SKColorType.RgbaF16Clamped)]
        [DataRow(SKColorType.RgbaF32)]
        //[DataRow(SKColorType.Rg88)] // not supported
        //[DataRow(SKColorType.AlphaF16)]
        //[DataRow(SKColorType.RgF16)] // not supported
        //[DataRow(SKColorType.Alpha16)]
        //[DataRow(SKColorType.Rg1616)] // not supported
        [DataRow(SKColorType.Rgba16161616)]
        [DataRow(SKColorType.Bgra1010102)]
        [DataRow(SKColorType.Bgr101010x)]
        //[DataRow(SKColorType.Bgr101010xXR)] // not supported
        [DataRow(SKColorType.Srgba8888)]
        //[DataRow(SKColorType.R8Unorm)] // not supported
        //[DataRow(SKColorType.Rgba10x6)] // not supported
        public void AsAuroraImage(SKColorType format)
        {
            using var imageBitmap = new SKBitmap(12, 12, format, SKAlphaType.Opaque);
            imageBitmap.SetPixel(1, 1, new SKColor(255, 0, 0));
            Assert.AreEqual(Red.ToScaledVector4(), imageBitmap.GetPixel(1, 1).ToBGRA32().ToScaledVector4());

            using (IImage imageAurora = imageBitmap.AsAuroraImage())
            {
                imageAurora[0, 0] = Black.ToScaledVector4();
                imageAurora[11, 0] = Red.ToScaledVector4();
                imageAurora[0, 11] = Green.ToScaledVector4();
                imageAurora[11, 11] = Blue.ToScaledVector4();
            }

            Assert.AreEqual(ToRGBA32(Black), imageBitmap.GetPixel(0, 0).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Red), imageBitmap.GetPixel(11, 0).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Green), imageBitmap.GetPixel(0, 11).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Blue), imageBitmap.GetPixel(11, 11).ToBGRA32());
        }

        [TestMethod]
        [DataRow(SKColorType.Rgb565)]
        [DataRow(SKColorType.Argb4444)]
        [DataRow(SKColorType.Rgba8888)]
        [DataRow(SKColorType.RgbaF16)]
        [DataRow(SKColorType.Bgra8888)]
        [DataRow(SKColorType.Rgba1010102)]
        public void CloneAsSKBitmap(SKColorType format)
        {
            using var imageAurora = new MemoryImage<RGBA32>(10, 10);
            imageAurora[0, 0] = Black;
            imageAurora[9, 0] = Red;
            imageAurora[0, 9] = Green;
            imageAurora[9, 9] = Blue;

            using var imageBitmap = imageAurora.CloneAsSKBitmap(format);
            Assert.AreEqual(ToRGBA32(Black), imageBitmap.GetPixel(0, 0).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Red), imageBitmap.GetPixel(9, 0).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Green), imageBitmap.GetPixel(0, 9).ToBGRA32());
            Assert.AreEqual(ToRGBA32(Blue), imageBitmap.GetPixel(9, 9).ToBGRA32());
        }

        public BGRA32 ToRGBA32(RGBA32 rgba)
        {
            BGRA32 bgra = default;
            bgra.From8Bit(rgba);
            return bgra;
        }
    }
}
