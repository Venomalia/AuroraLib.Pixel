using AuroraLib.Pixel.Processing.Resampler;
using System;
using System.Buffers;

namespace AuroraLib.Pixel.Processing.Helper
{
    public sealed class ResizeKernelMap2D : IDisposable
    {
        /// <summary>
        /// Sampling information for each destination pixel.
        /// </summary>
        public readonly Kernel[] Kernels;

        /// <summary>
        /// Weight buffer referenced by the kernels.
        /// </summary>
        public readonly float[] Weights;

        /// <summary>
        /// Maximum number of weights used by a single kernel.
        /// </summary>
        public readonly int MaxKernelLength;

        public ResizeKernelMap2D(int destinationSize, int sourceSize, IResampler resampler)
        {
            Kernels = new Kernel[destinationSize];

            double ratio = sourceSize / (double)destinationSize;
            double scale = Math.Max(ratio, 1);
            int radius = (int)Math.Ceiling(resampler.Radius * scale);

            // Find repeating sampling pattern between source and destination.
            int gcd = GreatestCommonDivisor(sourceSize, destinationSize);
            int period = destinationSize / gcd;
            int sourcePeriod = sourceSize / gcd;

            // Calculate the number of kernels affected by the image borders.
            double center = (ratio - 1) * 0.5;
            int cornerInterval = (int)Math.Ceiling((radius - center - 1) / ratio);

            // Only store weights for the borders and one complete repeating period.
            int requiredKernelCount = Math.Min(cornerInterval * 2 + period, destinationSize);
            bool usePeriod = requiredKernelCount < destinationSize;

            MaxKernelLength = radius * 2;
            Weights = ArrayPool<float>.Shared.Rent(requiredKernelCount * MaxKernelLength);

            int weightOffset = 0;
            if (usePeriod)
            {
                // Build left border and the first occurrence of the repeating pattern.
                int firstRepeat = cornerInterval + period;
                for (int i = 0; i < firstRepeat; i++)
                {
                    BuildKernel(Kernels, Weights, i, sourceSize, ratio, scale, radius, resampler, ref weightOffset);
                }

                // Reuse the repeating kernels by shifting their source offset.
                int bottomStart = destinationSize - cornerInterval;
                for (int i = firstRepeat; i < bottomStart; i++)
                {
                    Kernel previous = Kernels[i - period];
                    Kernels[i] = new Kernel(previous.Start + sourcePeriod, previous.Length, previous.WeightOffset);
                }

                // Build right border kernels.
                for (int i = 0; i < cornerInterval; i++)
                {
                    BuildKernel(Kernels, Weights, bottomStart + i, sourceSize, ratio, scale, radius, resampler, ref weightOffset);
                }
            }
            else
            {
                // No useful repeating pattern exists, so build every kernel individually.
                for (int i = 0; i < destinationSize; i++)
                {
                    BuildKernel(Kernels, Weights, i, sourceSize, ratio, scale, radius, resampler, ref weightOffset);
                }
            }
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }

            return a;
        }

        private static void BuildKernel(Span<Kernel> kernels, float[] weights, int index, int sourceSize, double ratio, double scale, int radius, IResampler resampler, ref int weightOffset)
        {
            double center = ((index + 0.5) * ratio) - 0.5;

            int start = Math.Max(0, (int)Math.Ceiling(center - radius));
            int end = Math.Min(sourceSize - 1, (int)Math.Floor(center + radius));

            int length = end - start + 1;

            kernels[index] = new Kernel(start, length, weightOffset);

            Span<float> kernelWeights = weights.AsSpan(weightOffset, length);

            float sum = 0;

            for (int x = 0; x < length; x++)
            {
                float weight = resampler.GetWeight((float)((start + x - center) / scale));

                kernelWeights[x] = weight;
                sum += weight;
            }

            if (sum > 0)
            {
                for (int x = 0; x < length; x++)
                {
                    kernelWeights[x] /= sum;
                }
            }

            weightOffset += length;
        }

        public void Dispose()
        {
            ArrayPool<float>.Shared.Return(Weights);
        }
    }
}
