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
        /// The region of the source image to be copied.
        /// </summary>
        private Rectangle Region { get; set; }

        /// <summary>
        /// The target coordinates (X, Y) on the destination image where the region will be copied.
        /// </summary>
        public (int X, int Y) TargetCoordinate { get; set; }

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
        /// <param name="targetCoordinate">The target coordinates (X, Y) on the destination image where the region will be copied.</param>
        /// <param name="mode">The blend mode to be applied during the copy operation.</param>
        public CopyRegionMaskProcessor(IReadOnlyImage source, IReadOnlyImage mask, Rectangle region, (int X, int Y) targetCoordinate, BlendModes.BlendFunction mode) : base(source, mask)
        {
            Region = region;
            TargetCoordinate = targetCoordinate;
            BlendMode = mode;
        }

        /// <inheritdoc/>
        protected override void Apply<TColorT, TColorS, TColorM>(IImage<TColorT> target, IReadOnlyImage<TColorS> source, IReadOnlyImage<TColorM> mask)
            => Copy(target, source, mask, Region, TargetCoordinate, BlendMode);

        internal static void Copy<TColorT, TColorS, TColorM>(IImage<TColorT> targetImage, IReadOnlyImage<TColorS> sourceImage, IReadOnlyImage<TColorM> maskImage, Rectangle srcRegion, (int X, int Y) targetCoordinate, BlendModes.BlendFunction blendMode)
           where TColorT : unmanaged, IColor<TColorT>
           where TColorS : unmanaged, IColor<TColorS>
           where TColorM : unmanaged, IColor<TColorM>
        {

            if (srcRegion.Width == 0 || srcRegion.Height == 0)
                return;

            if (srcRegion.X + srcRegion.Width > sourceImage.Width ||
                srcRegion.Y + srcRegion.Height > sourceImage.Height)
                throw new ArgumentOutOfRangeException(nameof(sourceImage), "Source region exceeds source image bounds.");

            if (srcRegion.X + srcRegion.Width > maskImage.Width ||
                srcRegion.Y + srcRegion.Height > maskImage.Height)
                throw new ArgumentOutOfRangeException(nameof(maskImage), "Mask region exceeds mask image bounds.");

            if (targetCoordinate.X + srcRegion.Width > targetImage.Width ||
                targetCoordinate.Y + srcRegion.Height > targetImage.Height)
                throw new ArgumentOutOfRangeException(nameof(targetImage), "Target region exceeds image bounds.");

            RowAccessor<TColorT> target = new RowAccessor<TColorT>(targetImage, targetCoordinate.X, srcRegion.Width);
            ReadOnlyRowAccessor<TColorS> source = new ReadOnlyRowAccessor<TColorS>(sourceImage, srcRegion.X, srcRegion.Width);
            ReadOnlyRowAccessor<TColorM> mask = new ReadOnlyRowAccessor<TColorM>(maskImage, srcRegion.X, srcRegion.Width);

            for (int y = 0; y < srcRegion.Height; y++)
            {
                Span<TColorT> targetRow = target[targetCoordinate.Y + y];
                ReadOnlySpan<TColorS> sourceRow = source[srcRegion.Y + y];
                ReadOnlySpan<TColorM> maskRow = mask[srcRegion.Y + y];

                targetRow.Blend(sourceRow, maskRow, blendMode);

                if (target.IsBuffered)
                {
                    target[srcRegion.Y + y] = targetRow;
                }
            }
        }
    }
}
