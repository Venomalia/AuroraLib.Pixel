using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Numerics;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only image with access to individual pixel colors.
    /// </summary>
    public interface IReadOnlyImage : IDisposable
    {
        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the pixel color at the specified coordinates as a scaled <see cref="Vector4"/>.
        /// Each component (X = R, Y = G, Z = B, W = A) is in the range [0, 1].
        /// </summary>
        /// <param name="x">The horizontal (X) coordinate of the pixel (0-based).</param>
        /// <param name="y">The vertical (Y) coordinate of the pixel (0-based).</param>
        /// <returns>A scaled <see cref="Vector4"/> representing the color of the pixel.</returns>
        Vector4 this[int x, int y] { get; }

        /// <summary>
        /// Applies a read-only pixel processor to the image.
        /// </summary>
        /// <param name="processor">The processor to apply.</param>
        void Apply(IReadOnlyPixelProcessor processor);

        /// <summary>
        /// Creates and returns a clone of the current image.
        /// </summary>
        /// <returns>A new <see cref="IImage"/> that is a clone of the current image.</returns>
        IImage Clone();
    }
}
