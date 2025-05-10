using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a mutable 2D image with a specific pixel format, allowing both reading and writing of pixel data.
    /// </summary>
    /// <typeparam name="TColor">
    /// The color type of the image. Must be unmanaged and implement <see cref="IColor{TColor}"/>.
    /// </typeparam>
    public interface IImage<TColor> : IReadOnlyImage<TColor>, IImage where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets or sets the pixel <typeparamref name="TColor"/> at the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal pixel coordinate.</param>
        /// <param name="y">The vertical pixel coordinate.</param>
        /// <returns>The pixel color at the given coordinates.</returns>
        new TColor this[int x, int y] { get; set; }

        /// <summary>
        /// Sets the <typeparamref name="TColor"/> of pixels in the specified row and column.
        /// </summary>
        /// <param name="x">The horizontal pixel coordinate.</param>
        /// <param name="y">The vertical pixel coordinate.</param>
        /// <param name="pixelRow">A read-only span containing pixel values to be written to the specified row.</param>
        void SetPixel(int x, int y, ReadOnlySpan<TColor> pixelRow);
    }
}
