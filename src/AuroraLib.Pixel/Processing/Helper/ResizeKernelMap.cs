using AuroraLib.Pixel.Processing.Resampler;
using System;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Helper
{
    public sealed class ResizeKernelMap : IDisposable
    {
        public readonly ResizeKernelMap2D X;
        public readonly ResizeKernelMap2D Y;

        public ResizeKernelMap(Size destination, Size source, IResampler resampler)
        {
            // Build horizontal resize kernels.
            X = new ResizeKernelMap2D(destination.Width, source.Width, resampler);

            // Build vertical resize kernels.
            // If both dimensions use the same ratio, the kernel map can be shared.
            if (destination.Width == destination.Height && source.Width == source.Height)
                Y = X;
            else
                Y = new ResizeKernelMap2D(destination.Height, source.Height, resampler);
        }

        public void Dispose()
        {
            X.Dispose();

            if (!ReferenceEquals(X, Y))
                Y.Dispose();
        }
    }
}
