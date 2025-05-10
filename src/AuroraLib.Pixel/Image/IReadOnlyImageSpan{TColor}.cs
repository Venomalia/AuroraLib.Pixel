using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only image with span-based access to raw pixel data and stride information.
    /// </summary>
    /// <typeparam name="TColor">The color type of the image.</typeparam>
    public interface IReadOnlyImageSpan<TColor> : IReadOnlyImage<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the number of elements between the start of one row and the start of the next.
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// Gets a read-only span over the raw pixel data.
        /// </summary>
        ReadOnlySpan<TColor> Pixel { get; }
    }
}
