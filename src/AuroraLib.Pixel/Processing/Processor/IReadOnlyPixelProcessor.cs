using AuroraLib.Pixel.Image;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Defines a processor that can be applied to an <see cref="IReadOnlyImage"/> to perform operations that do not modify the pixels.
    /// </summary>
    public interface IReadOnlyPixelProcessor
    {
        /// <summary>
        /// Applies the processor logic to the specified image without modifying its pixels.
        /// <para>
        /// To use this method, call <see cref="IReadOnlyImage.Apply(IReadOnlyPixelProcessor)"/> on an image instance.
        /// </para>
        /// </summary>
        /// <typeparam name="TColor">The pixel color type.</typeparam>
        /// <param name="image">The image to process.</param>
        public void Apply<TColor>(IReadOnlyImage<TColor> image) where TColor : unmanaged, IColor<TColor>;
    }
}
