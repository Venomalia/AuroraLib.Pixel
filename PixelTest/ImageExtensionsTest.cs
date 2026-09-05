using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using RGBA32 = AuroraLib.Pixel.PixelFormats.RGBA<byte>;

namespace PixelTest
{
    [TestClass]
    public class ImageExtensionsTest
    {
        [TestMethod]
        [DataRow(0, 0)]
        [DataRow(5, 0)]
        [DataRow(0, 5)]
        [DataRow(-5, 0)]
        [DataRow(0, -5)]
        [DataRow(5, 5)]
        [DataRow(-5, 5)]
        [DataRow(5, -5)]
        [DataRow(-5, -5)]
        [DataRow(-10, 0)]
        public void CopyFrom_ClipsToTarget(int x, int y)
        {
            var imageA = new MemoryImage<RGBA32>(10, 10);
            var imageB = new MemoryImage<RGBA32>(10, 10);
            imageA.Pixel.Fill(Color.White);
            imageB.Pixel.Fill(Color.Black);

            imageA.CopyFrom(imageB, new Point(x, y));

            for (int py = 0; py < 10; py++)
            {
                for (int px = 0; px < 10; px++)
                {
                    bool inside = px >= x && px < x + 10 && py >= y && py < y + 10;
                    RGBA32 expected = inside ? Color.Black : Color.White;
                    Assert.AreEqual(expected, imageA[px, py]);
                }
            }
        }
    }
}
