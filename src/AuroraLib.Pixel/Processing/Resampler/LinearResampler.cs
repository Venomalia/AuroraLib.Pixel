using System;

namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Resampling filter using linear interpolation.
    /// The underlying triangle kernel smoothly interpolates between neighboring
    /// source pixels.
    /// </summary>
    public sealed class LinearResampler : IResampler
    {
        /// <inheritdoc/>
        public float Radius => 1f;

        /// <inheritdoc/>
        public float GetWeight(float x)
        {
            x = Math.Abs(x);
            return x < 1f ? 1f - x : 0f;
        }

    }
}
