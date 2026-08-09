namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Defines a one-dimensional resampling filter used for image scaling.
    /// </summary>
    public interface IResampler
    {
        /// <summary>
        /// Gets the support radius of the resampling filter in source pixels.
        /// Pixels farther away than this distance have a weight of zero.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Returns the interpolation weight for the specified distance from the sampling position.
        /// </summary>
        /// <param name="distance">
        /// The absolute distance, in source pixels, from the sampling position.
        /// </param>
        /// <returns>
        /// The interpolation weight for the specified distance.
        /// </returns>
        float GetWeight(float distance);
    }
}
