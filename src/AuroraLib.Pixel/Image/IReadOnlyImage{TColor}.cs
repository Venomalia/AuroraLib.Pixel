using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only image with a specific pixel format.
    /// Provides type-safe access to pixel data using a generic color type.
    /// </summary>
    /// <typeparam name="TColor">
    /// The color type of the image. Must be unmanaged and implement <see cref="IColor{TColor}"/>.
    /// </typeparam>
    public interface IReadOnlyImage<TColor> : IReadOnlyImage where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the <typeparamref name="TColor"/> of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal pixel coordinate.</param>
        /// <param name="y">The vertical pixel coordinate.</param>
        /// <returns>The color of the pixel at the given coordinates.</returns>
        new TColor this[int x, int y] { get; }

        /// <summary>
        /// Retrieves the <typeparamref name="TColor"/> of pixels in the specified row and column.
        /// </summary>
        /// <param name="x">The horizontal pixel coordinate.</param>
        /// <param name="y">The vertical pixel coordinate.</param>
        /// <param name="pixelRow">A span that will be filled with pixel values from the specified row.</param>
        void GetPixel(int x, int y, Span<TColor> pixelRow);

        /// <inheritdoc cref="IReadOnlyImage.Clone()"/>
        new IImage<TColor> Clone();
    }
}
