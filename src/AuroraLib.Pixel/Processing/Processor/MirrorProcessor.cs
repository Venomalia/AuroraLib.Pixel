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
        /// Gets or sets the rectangle region of the image to be mirrored.
        /// </summary>
        public Rectangle Region { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MirrorProcessor"/> class with the specified axis and rectangle.
        /// </summary>
        /// <param name="mirroring">The axis along which the image will be mirrored.</param>
        /// <param name="region">The rectangle that defines the region of the image to be mirrored.</param>
        public MirrorProcessor(MirrorAxis mirroring, Rectangle region)
        {
            Mirroring = mirroring;
            Region = region;
        }

        /// <inheritdoc/>
        public void Apply<TColor>(IImage<TColor> image) where TColor : unmanaged, IColor<TColor>
        {
            if (Mirroring == MirrorAxis.None || Region.Width == 0 || Region.Height == 0)
                return;

            if (image is IPaletteImage<TColor> paletteImage)
            {
                paletteImage.GetBuffer().Apply(this);
                return;
            }

            image.Mirror(Mirroring, Region);
        }
    }
}
