using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Drawing;
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
        /// Gets the metadata associated with the image,
        /// </summary>
        ImageMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets the pixel format metadata for this color.
        /// </summary>
        PixelFormatInfo PixelFormat { get; }

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
        /// <param name="region">The area of the source image to be processed.</param>
        void Apply(IReadOnlyPixelProcessor processor, Rectangle region);

        /// <summary>
        /// Creates and returns a clone of the current image.
        /// </summary>
        /// <returns>A new <see cref="IImage"/> that is a clone of the current image.</returns>
        IImage Clone();

        /// <summary>
        /// Creates a copy of the specified image region using the specified target color format.
        /// </summary>
        /// <typeparam name="TColor">The color type of the cloned image.</typeparam>
        /// <param name="region">The region of the image to clone.</param>
        /// <returns>A new <see cref="IImage"/> containing the copied region in the specified color format.</returns>
        IImage<TColor> CloneAs<TColor>(Rectangle region) where TColor : unmanaged, IColor<TColor>;

        /// <summary>
        /// Creates a new image instance with the specified dimensions.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        IImage Create(int width, int height);
    }
}
