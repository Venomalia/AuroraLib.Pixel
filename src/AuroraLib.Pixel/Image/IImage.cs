using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Processor;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a mutable image, allowing both reading and writing of pixel data.
    /// </summary>
    public interface IImage : IReadOnlyImage
    {
        /// <summary>
        /// Gets or sets the pixel color at the specified coordinates as a scaled <see cref="Vector4"/>.
        /// Each component (X = R, Y = G, Z = B, W = A) is in the range [0, 1].
        /// </summary>
        /// <param name="x">The horizontal (X) coordinate of the pixel (0-based).</param>
        /// <param name="y">The vertical (Y) coordinate of the pixel (0-based).</param>
        /// <returns>A <see cref="Vector4"/> representing the color at the specified pixel.</returns>
        new Vector4 this[int x, int y] { get; set; }

        /// <summary>
        /// Applies a pixel processor that can modify the image.
        /// </summary>
        /// <param name="processor">The processor to apply to the image.</param>
        void Apply(IPixelProcessor processor);

        /// <summary>
        /// Clears the entire image by setting all pixels to the default value.
        /// </summary>
        void Clear();

        /// <summary>
        /// Crops the image to the specified region.
        /// </summary>
        /// <param name="region">The region to crop. This should specify the top-left corner (X, Y) and the width and height of the region to retain.</param>
        void Crop(Rectangle region);
    }
}
