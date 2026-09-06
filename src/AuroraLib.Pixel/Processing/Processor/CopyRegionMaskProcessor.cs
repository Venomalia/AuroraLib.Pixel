using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using System;
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
        public CopyRegionMaskProcessor(IReadOnlyImage source, IReadOnlyImage mask, Rectangle region, BlendModes.BlendFunction mode) : base(source, mask, region, region)
            => BlendMode = mode;

        /// <inheritdoc/>
        protected override void Apply<TColorT, TColorS, TColorM>(IImage<TColorT> target, IReadOnlyImage<TColorS> source, IReadOnlyImage<TColorM> mask, Rectangle targetRegion, Rectangle sourceRegion, Rectangle _)
            => Copy(target, source, mask, sourceRegion, targetRegion.Location, BlendMode);

        internal static void Copy<TColorT, TColorS, TColorM>(IImage<TColorT> target, IReadOnlyImage<TColorS> source, IReadOnlyImage<TColorM> mask, Rectangle srcRegion, Point targetCoordinate, BlendModes.BlendFunction blendMode)
           where TColorT : unmanaged, IColor<TColorT>
           where TColorS : unmanaged, IColor<TColorS>
           where TColorM : unmanaged, IColor<TColorM>
        {

            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(srcRegion), "Region exceeds source image bounds.");

            if (!mask.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(srcRegion), "region exceeds mask image bounds.");

            if (targetCoordinate.X + srcRegion.Width > target.Width || targetCoordinate.Y + srcRegion.Height > target.Height)
                throw new ArgumentOutOfRangeException(nameof(target), "Target region exceeds image bounds.");

            if (srcRegion.Width == 0 || srcRegion.Height == 0)
                return;

            RowAccessor<TColorT> targetPixel = new RowAccessor<TColorT>(target, targetCoordinate.X, srcRegion.Width);
            ReadOnlyRowAccessor<TColorS> sourcePixel = new ReadOnlyRowAccessor<TColorS>(source, srcRegion.X, srcRegion.Width);
            ReadOnlyRowAccessor<TColorM> maskPixel = new ReadOnlyRowAccessor<TColorM>(mask, srcRegion.X, srcRegion.Width);

            for (int y = 0; y < srcRegion.Height; y++)
            {
                Span<TColorT> targetRow = targetPixel[targetCoordinate.Y + y];
                ReadOnlySpan<TColorS> sourceRow = sourcePixel[srcRegion.Y + y];
                ReadOnlySpan<TColorM> maskRow = maskPixel[srcRegion.Y + y];

                targetRow.Blend(sourceRow, maskRow, blendMode);

                if (targetPixel.IsBuffered)
                {
                    targetPixel[srcRegion.Y + y] = targetRow;
                }
            }
        }
    }
}
