using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a writable image with span-based access to raw pixel data and stride information.
    /// </summary>
    /// <typeparam name="TColor">The color type of the image.</typeparam>
    public interface IImageSpan<TColor> : IImage<TColor>, IReadOnlyImageSpan<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets a span over the raw pixel data that can be modified directly.
        /// </summary>
        new Span<TColor> Pixel { get; }
    }
}
