using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Helper;
using AuroraLib.Pixel.Processing.Resampler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using RGBA32 = AuroraLib.Pixel.PixelFormats.RGBA<byte>;

namespace PixelTest
{
    [TestClass]
    public class ResamplerTest
    {
        private readonly RGBA32 Black = uint.MaxValue;
        private readonly RGBA32 Red = 0xFF0000FF;
        private readonly RGBA32 Green = 0xFF00FF00;
        private readonly RGBA32 Blue = 0xFFFF0000;

        [TestMethod]
        public void BoxResampler_ShouldReturnExpectedWeights()
        {
            BoxResampler resampler = new BoxResampler();

            Assert.AreEqual(1f, resampler.GetWeight(0f), 0.00001f);

            Assert.AreEqual(1f, resampler.GetWeight(0.25f), 0.00001f);
            Assert.AreEqual(1f, resampler.GetWeight(0.5f), 0.00001f);

            Assert.AreEqual(0f, resampler.GetWeight(-0.5f), 0.00001f);
            Assert.AreEqual(0f, resampler.GetWeight(0.50001f), 0.00001f);
        }

        [TestMethod]
        public void LinearResampler_ShouldReturnExpectedWeights()
        {
            LinearResampler resampler = new LinearResampler();

            Assert.AreEqual(1f, resampler.GetWeight(0f), 0.00001f);

            Assert.AreEqual(0.75f, resampler.GetWeight(0.25f), 0.00001f);
            Assert.AreEqual(0.5f, resampler.GetWeight(0.5f), 0.00001f);
            Assert.AreEqual(0.25f, resampler.GetWeight(0.75f), 0.00001f);

            Assert.AreEqual(0f, resampler.GetWeight(1f), 0.00001f);
            Assert.AreEqual(0f, resampler.GetWeight(-1f), 0.00001f);

            Assert.AreEqual(resampler.GetWeight(0.5f), resampler.GetWeight(-0.5f), 0.00001f);
            Assert.AreEqual(resampler.GetWeight(0.75f), resampler.GetWeight(-0.75f), 0.00001f);
        }

        [TestMethod]
        public void CubicResampler_ShouldReturnExpectedWeights()
        {
            CubicResampler resampler = new CubicResampler();

            Assert.AreEqual(1f, resampler.GetWeight(0f), 0.00001f);
            Assert.AreEqual(0f, resampler.GetWeight(2f), 0.00001f);
            Assert.AreEqual(0f, resampler.GetWeight(-2f), 0.00001f);
            Assert.AreEqual(resampler.GetWeight(0.5f), resampler.GetWeight(-0.5f), 0.00001f);
            Assert.AreEqual(resampler.GetWeight(1f), resampler.GetWeight(-1f), 0.00001f);

            Assert.IsTrue(resampler.GetWeight(0.5f) > resampler.GetWeight(1f));
        }

        [TestMethod]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(6)]
        public void LanczosResampler_ShouldReturnExpectedWeights(int lobes)
        {
            LanczosResampler resampler = new LanczosResampler(lobes);

            Assert.AreEqual(1f, resampler.GetWeight(0f), 0.00001f);

            Assert.AreEqual(0f, resampler.GetWeight(lobes), 0.00001f);
            Assert.AreEqual(0f, resampler.GetWeight(-lobes), 0.00001f);

            Assert.AreEqual(resampler.GetWeight(0.5f), resampler.GetWeight(-0.5f), 0.00001f);
            Assert.AreEqual(resampler.GetWeight(1f), resampler.GetWeight(-1f), 0.00001f);
            Assert.AreEqual(resampler.GetWeight(lobes - 0.5f), resampler.GetWeight(-(lobes - 0.5f)), 0.00001f);
        }

        [TestMethod]
        [DataRow(10, 10, 5, 7)]
        [DataRow(7, 5, 10, 10)]
        [DataRow(10, 15, 99, 124)]
        [DataRow(77, 88, 1500, 1500)]
        [DataRow(192, 108, 128, 72)]
        [DataRow(512, 512, 128, 128)]
        [DataRow(1, 1, 100, 100)]
        [DataRow(100, 100, 1, 1)]
        public void KernelMap_Cubic_ShouldCreateValidKernels(int sWidth, int sHeight, int dWidth, int dHeight)
        {
            Test(new Size(sWidth, sHeight), new Size(dWidth, dHeight), CubicResampler.CatmullRom);
        }

        private static void Test(Size source, Size destination, IResampler resampler)
        {
            using ResizeKernelMap map = new ResizeKernelMap(destination, source, resampler);

            CheckAxis(map.X, source.Width, "X");
            CheckAxis(map.Y, source.Height, "Y");
        }

        private static void CheckAxis(ResizeKernelMap2D map, float sourceSize, string axis)
        {
            var kernels = map.Kernels;
            var weights = map.Weights;
            for (int i = 0; i < kernels.Length; i++)
            {
                Kernel kernel = kernels[i];
                ReadOnlySpan<float> kernelWeights = weights.AsSpan(kernel.WeightOffset, kernel.Length);

                Assert.IsTrue(kernel.Start >= 0, $"{axis}: Kernel {i} has invalid start.");
                Assert.IsTrue(kernel.Length > 0, $"{axis}: Kernel {i} has invalid length.");
                Assert.IsTrue(kernel.WeightOffset >= 0, $"{axis}: Kernel {i} has invalid weight offset.");
                Assert.IsTrue(kernel.Start + kernel.Length <= sourceSize, $"{axis}: Kernel {i} exceeds source bounds.");

                float sum = 0;
                for (int j = 0; j < kernel.Length; j++)
                {
                    Assert.IsFalse(float.IsNaN(kernelWeights[j]));
                    Assert.IsFalse(float.IsInfinity(kernelWeights[j]));
                    sum += kernelWeights[j];
                }

                Assert.AreEqual(1f, sum, 0.00001f, $"{axis}: Kernel {i} weights are not normalized.");
            }
        }

        [TestMethod]
        public void Resize_NearestNeighborResampler()
        {
            using var source = new MemoryImage<RGBA32>(10, 10);
            using var target = new MemoryImage<RGBA32>(20, 20);

            source[0, 0] = Black;
            source[9, 0] = Red;
            source[0, 9] = Green;
            source[9, 9] = Blue;

            target.ResizeFrom(source, source.GetBounds(), target.GetBounds(), new NearestNeighborResampler());

            Assert.AreEqual(Black, target[0, 0]);
            Assert.AreEqual(Red, target[19, 0]);
            Assert.AreEqual(Green, target[0, 19]);
            Assert.AreEqual(Blue, target[19, 19]);
        }

        [TestMethod]
        public void Resize_WithKernelResampler()
        {
            using var source = new MemoryImage<RGBA32>(2, 2);
            using var target = new MemoryImage<RGBA32>(4, 4);

            source[0, 0] = Black;
            source[1, 0] = Red;
            source[0, 1] = Green;
            source[1, 1] = Blue;

            target.ResizeFrom(source, source.GetBounds(), target.GetBounds(), new LinearResampler());

            Assert.AreEqual(Black, target[0, 0]);
            Assert.AreEqual(Red, target[3, 0]);
            Assert.AreEqual(Green, target[0, 3]);
            Assert.AreEqual(Blue, target[3, 3]);
        }
    }
}
