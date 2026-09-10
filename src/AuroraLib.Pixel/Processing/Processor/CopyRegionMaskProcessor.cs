using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Represents a processor that copies a region from one source image to a target image at a specific location,using a mask.
    /// </summary>
    public sealed class CopyRegionMaskProcessor : TripleImageProcessor
    {
        /// <summary>
        /// The blend mode to be applied during the copy operation.
        /// </summary>
        public BlendModes.BlendFunction BlendMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRegionMaskProcessor"/> class.
        /// </summary>
        /// <param name="source">The source image from which the region will be copied.</param>
        /// <param name="mask">The mask image used to determine how blending is applied during the copy operation.</param>
        /// <param name="region">The region of the source image to be copied.</param>
        /// <param name="mode">The blend mode to be applied during the copy operation.</param>
        public CopyRegionMaskProcessor(IReadOnlyImage source, IReadOnlyImage mask, Rectangle region, Point maskOffset, BlendModes.BlendFunction mode) : base(source, mask, region, new Rectangle(maskOffset, region.Size))
            => BlendMode = mode;

        /// <inheritdoc/>
        protected override void Apply<TColorT, TColorS, TColorM>(IImage<TColorT> target, IReadOnlyImage<TColorS> source, IReadOnlyImage<TColorM> mask, Rectangle targetRegion, Rectangle sourceRegion, Rectangle maskRegion)
            => target.CopyFrom(source, mask, sourceRegion, targetRegion.Location, maskRegion.Location, BlendMode);

    }
}
