using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Represents a processor that copies a region from one <see cref="IReadOnlyImage"/> to a target location on another <see cref="IImage"/>.
    /// </summary>
    public sealed class CopyRegionProcessor : DoubleImageProcessor
    {
        /// <summary>
        /// The blend mode to be applied during the copy operation. If <c>null</c>, no blending is applied.
        /// </summary>
        public BlendModes.BlendFunction? BlendMode { get; set; }

        /// <summary>
        /// The intensity of the blending.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRegionProcessor"/> class with specified source image, region, and target coordinates.
        /// </summary>
        /// <param name="source">The source image from which to copy a region.</param>
        /// <param name="region">The region of the source image to copy.</param>
        /// <param name="targetCoordinate">The target coordinates (X, Y) on the destination image.</param>
        /// <param name="mode">An optional blend mode to apply while copying the region. If <c>null</c>, no blending is performed.</param>
        /// <param name="intensity">The intensity of the blending.</param>
        public CopyRegionProcessor(IReadOnlyImage source, Rectangle region, BlendModes.BlendFunction? mode = null, float intensity = 1f) : base(source, region)
        {
            BlendMode = mode;
            Intensity = intensity;
        }

        public CopyRegionProcessor(IReadOnlyImage source, BlendModes.BlendFunction? mode = null, float intensity = 1f) : this(source, source.GetBounds(), mode, intensity)
        { }

        /// <inheritdoc/>
        protected override void Apply<TColorT, TColorS>(IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle targetRegion, Rectangle sourceRegion)
            => target.CopyFrom(source, sourceRegion, targetRegion.Location, BlendMode, Intensity);
    }
}
