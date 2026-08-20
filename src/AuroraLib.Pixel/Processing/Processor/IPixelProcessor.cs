using AuroraLib.Pixel.Image;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Defines a processor that can be applied to an <see cref="IImage"/> to perform pixel-wise operations.
    /// </summary>
    public interface IPixelProcessor
    {
        /// <summary>
        /// Applies the pixel processing logic to the specified image.
        /// <para>
        /// To use this method, call <see cref="IImage.Apply(IPixelProcessor, Rectangle)"/> on an image instance.
        /// </para>
        /// </summary>
        /// <typeparam name="TColor">The pixel color type.</typeparam>
        /// <param name="image">The image to process.</param>
        /// <param name="region">The area of the source image to be processed.</param>
        public void Apply<TColor>(IImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>;
    }
}
