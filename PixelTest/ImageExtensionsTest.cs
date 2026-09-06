using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Resampler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Numerics;
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
            using var imageA = new MemoryImage<RGBA32>(10, 10);
            using var imageB = new MemoryImage<RGBA32>(10, 10);
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

        [TestMethod]
        public void Transform_Identity()
        {
            using var imageA = new MemoryImage<RGBA32>(5, 5);
            using var imageB = new MemoryImage<RGBA32>(3, 3);

            imageA.Pixel.Fill(Color.White);
            imageB.Pixel.Fill(Color.Black);
            imageB[0, 0] = Color.Red;
            imageB[2, 0] = Color.Green;
            imageB[0, 2] = Color.Blue;
            imageB[2, 2] = Color.Magenta;

            imageA.Transform(imageB, new Rectangle(1, 1, 3, 3), Matrix3x2.Identity, Resamplers.NearestNeighbor);

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    RGBA32 expected = x >= 1 && x < 4 && y >= 1 && y < 4 ? imageB[x - 1, y - 1] : (RGBA32)Color.White;

                    Assert.AreEqual(expected, imageA[x, y], $"Pixel ({x}, {y})");
                }
            }
        }

        [TestMethod]
        public void Transform_Rotate90()
        {
            using var imageA = new MemoryImage<RGBA32>(3, 3);
            using var imageB = new MemoryImage<RGBA32>(3, 3);

            imageA.Pixel.Fill(Color.White);
            imageB.Pixel.Fill(Color.Black);

            imageB[0, 0] = Color.Red;
            imageB[2, 0] = Color.Green;
            imageB[0, 2] = Color.Blue;
            imageB[2, 2] = Color.Magenta;

            Matrix3x2 transform = Matrix3x2.CreateRotation((float)Math.PI / 2, new Vector2(1.5f, 1.5f));

            imageA.Transform(imageB, transform);

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    RGBA32 expected = imageB[y, 2 - x];

                    Assert.AreEqual(expected, imageA[x, y], $"Pixel ({x}, {y})");
                }
            }
        }

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
        public void Transform_Translation(int x, int y)
        {
            using var imageA = new MemoryImage<RGBA32>(10, 10);
            using var imageB = new MemoryImage<RGBA32>(10, 10);
            imageA.Pixel.Fill(Color.White);
            imageB.Pixel.Fill(Color.Black);

            Matrix3x2 transform = Matrix3x2.CreateTranslation(new Vector2(x, y));
            imageA.Transform(imageB, transform);

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

        [TestMethod]
        public void Transform_Scale()
        {
            using var imageA = new MemoryImage<RGBA32>(8, 8);
            using var imageB = new MemoryImage<RGBA32>(4, 4);
            imageA.Pixel.Fill(Color.White);
            imageB.Pixel.Fill(Color.Black);
            imageB[0, 0] = Color.Red;
            imageB[3, 0] = Color.Green;
            imageB[0, 3] = Color.Blue;
            imageB[3, 3] = Color.Magenta;

            Matrix3x2 transform = Matrix3x2.CreateScale(2f);

            imageA.Transform(imageB, transform, Resamplers.NearestNeighbor);

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    RGBA32 expected = imageB[x / 2, y / 2];

                    Assert.AreEqual(expected, imageA[x, y], $"Pixel ({x}, {y})");
                }
            }
        }
    }
}
