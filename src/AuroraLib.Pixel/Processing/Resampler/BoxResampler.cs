namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Resampling filter using a box kernel.
    /// All source samples inside the filter area contribute with equal weight, producing an averaging effect when reducing image size.
    /// </summary>
    public sealed class BoxResampler : IResampler
    {
        /// <inheritdoc/>
        public float Radius => 0.5f;

        /// <inheritdoc/>
        public float GetWeight(float x) => x > -0.5F && x <= 0.5F ? 1 : 0;
    }
}
