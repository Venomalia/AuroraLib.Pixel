using System;

namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Represents a Lanczos resampling filter based on a windowed sinc function.
    /// It provides high-quality interpolation with a good balance between sharpness and smoothness for image resizing.
    /// </summary>
    public sealed class LanczosResampler : IResampler
    {
        private readonly int lobes;

        /// <inheritdoc/>
        public float Radius => lobes;

        public LanczosResampler(int lobes = 3)
        {
                if (lobes < 1) throw new ArgumentOutOfRangeException(nameof(lobes));
                this.lobes = lobes;
        }

        /// <inheritdoc/>
        public float GetWeight(float x)
        {
            x = Math.Abs(x);

            if (x == 0f)
                return 1f;

            if (x >= lobes)
                return 0f;

            float pix = (float)Math.PI * x;

            return SinC(pix) * SinC(pix / lobes);

            static float SinC(float x)
#if NET6_0_OR_GREATER
                => MathF.Sin(x) / x;
#else
                => (float)(Math.Sin(x) / x);
#endif
        }
    }
}
