using AuroraLib.Pixel.Image;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// A processor that mirrors a specified region of an image along a given axis.
    /// </summary>
    public sealed class MirrorProcessor : IPixelProcessor
    {
        /// <summary>
        /// Gets or sets the axis along which the image should be mirrored.
        /// </summary>
        public MirrorAxis Mirroring { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MirrorProcessor"/> class with the specified axis and rectangle.
        /// </summary>
        /// <param name="mirroring">The axis along which the image will be mirrored.</param>
        public MirrorProcessor(MirrorAxis mirroring) => Mirroring = mirroring;

        /// <inheritdoc/>
        public void Apply<TColor>(IImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            if (Mirroring == MirrorAxis.None || region.Width == 0 || region.Height == 0)
                return;

            if (image is IPaletteImage<TColor> paletteImage)
            {
                // We mirror the values directly in the index image.
                paletteImage.ApplyToIndices(this, region);
                return;
            }

            image.Mirror(Mirroring, region);
        }
    }
}
