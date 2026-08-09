using System;
using System.ComponentModel;

namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Resampling filter that selects the source pixel closest to the sampling position.
    /// This filter produces sharp results but does not perform interpolation.
    /// </summary>
    public sealed class NearestNeighborResampler : IResampler
    {
        /// <inheritdoc/>
        public float Radius => 0.5f;

        /// <inheritdoc/>
        [Obsolete("Nearest neighbor does not use weights and does not support kernel generation.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetWeight(float distance) => throw new NotSupportedException();
    }
}
