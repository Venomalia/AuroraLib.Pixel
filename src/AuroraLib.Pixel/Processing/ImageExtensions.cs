using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Drawing;

namespace AuroraLib.Pixel.Processing
{
    /// <summary>
    /// ImageExtensions
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Determines if the specified rectangle is entirely within the bounds of the image.
        /// </summary>
        /// <param name="image">The image to check against.</param>
        /// <param name="rectangle">The rectangle to check.</param>
        /// <returns>True if the rectangle is fully contained within the image; otherwise, false.</returns>
        public static bool Contains(this IReadOnlyImage image, Rectangle rectangle)
            => rectangle.X >= 0 && rectangle.Y >= 0 && rectangle.Right <= image.Width && rectangle.Bottom <= image.Height;

        /// <summary>
        /// Determines if the specified point is within the bounds of the image.
        /// </summary>
        /// <param name="image">The image to check against.</param>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is within the image; otherwise, false.</returns>
        public static bool Contains(this IReadOnlyImage image, Point point)
            => point.X >= 0 && point.X < image.Width && point.Y >= 0 && point.Y < image.Height;

        /// <summary>
        /// Clones the <paramref name="source"/> <see cref="IReadOnlyImage"/> and converts it to a new <see cref="IImage"/> of type <typeparamref name="TColor"/>.
        /// </summary>
        /// <typeparam name="TColor">The color type to which the image is cloned.</typeparam>
        /// <param name="source">The source image to clone.</param>
        /// <returns>A new <see cref="IImage{TColor}"/> that is a clone of the source image.</returns>
        public static IImage<TColor> CloneAs<TColor>(this IReadOnlyImage source) where TColor : unmanaged, IColor<TColor>
        {
            if (source is IReadOnlyImage<TColor> sourceOfT)
                return sourceOfT.Clone();

            MemoryImage<TColor> clone = new MemoryImage<TColor>(source.Width, source.Height);
            clone.CopyFrom(source);
            return clone;
        }

        /// <summary>
        /// Clones a specific region of the <paramref name="source"/> <see cref="IReadOnlyImage"/> and converts it to a new <see cref="IImage"/> of type <typeparamref name="TColor"/>.
        /// </summary>
        /// <typeparam name="TColor">The color type to which the image region is cloned.</typeparam>
        /// <param name="source">The source image to clone from.</param>
        /// <param name="region">The region of the source image to clone.</param>
        /// <returns>A new <see cref="IImage{TColor}"/> that is a clone of the specified region of the source image.</returns>
        public static IImage<TColor> CloneAs<TColor>(this IReadOnlyImage source, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            MemoryImage<TColor> clone = new MemoryImage<TColor>(region.Width, region.Height);
            clone.CopyFrom(source, region, default);
            return clone;
        }

        /// <summary>
        /// Copies a region from a <paramref name="source"/> image to a <paramref name="target"/> image, with an optional <paramref name="blendMode"/> and <paramref name="intensity"/> for blending.
        /// </summary>
        /// <typeparam name="TColorT">The color type of the target image.</typeparam>
        /// <typeparam name="TColorS">The color type of the source image.</typeparam>
        /// <param name="target">The image that will receive the copied region.</param>
        /// <param name="source">The source image to copy from.</param>
        /// <param name="srcRegion">The region in the source image to copy.</param>
        /// <param name="targetCoordinate">The coordinates (X, Y) in the target image where the region will be copied to.</param>
        /// <param name="blendMode">An optional blend mode to apply while copying the region. If <c>null</c>, no blending is performed.</param>
        /// <param name="intensity">The intensity of the blending (from 0 to 1).</param>
        public static void CopyFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle srcRegion, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT>
            where TColorS : unmanaged, IColor<TColorS>
        {
            if (srcRegion.Width == 0 || srcRegion.Height == 0)
                return;

            EnsureValidDimensions(ref srcRegion);

            if (!source.Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(source), "Region exceeds source image bounds.");

            if (!source.Contains(new Rectangle(targetCoordinate, srcRegion.Size)))
                throw new ArgumentOutOfRangeException(nameof(target), "Region exceeds target image bounds.");

            RowAccessor<TColorT> targetPixel = new RowAccessor<TColorT>(target, targetCoordinate.X, srcRegion.Width);
            ReadOnlyRowAccessor<TColorS> sourcePixel = new ReadOnlyRowAccessor<TColorS>(source, srcRegion.X, srcRegion.Width);

            for (int y = 0; y < srcRegion.Height; y++)
            {
                Span<TColorT> targetRow = targetPixel[targetCoordinate.Y + y];
                ReadOnlySpan<TColorS> sourceRow = sourcePixel[srcRegion.Y + y];

                if (blendMode is null)
                    sourceRow.To(targetRow);
                else
                    targetRow.Blend(sourceRow, blendMode, intensity);

                if (targetPixel.IsBuffered)
                {
                    targetPixel[srcRegion.Y + y] = targetRow;
                }
            }
        }

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.CopyFrom(source, new Rectangle(0, 0, source.Width, source.Height), default, blendMode, intensity);

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, Rectangle srcRegion, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, srcRegion, targetCoordinate, blendMode, intensity));

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, blendMode, intensity));

        /// <summary>
        /// Mirrors the <paramref name="image"/> along the specified axis within a given <paramref name="region"/>.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image.</typeparam>
        /// <param name="image">The image to mirror.</param>
        /// <param name="mirroring">The axis along which to mirror the image (Horizontal, Vertical, or Both).</param>
        /// <param name="region">The region of the image to apply the mirroring to.</param>
        public static void Mirror<TColor>(this IImage<TColor> image, MirrorAxis mirroring, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            if (mirroring == MirrorAxis.None || region.Width == 0 || region.Height == 0)
                return;

            EnsureValidDimensions(ref region);

            if (!image.Contains(region))
                throw new ArgumentOutOfRangeException(nameof(region), "Region exceeds image bounds.");

            RowAccessor<TColor> topRow = new RowAccessor<TColor>(image, region.X, region.Width);

            if (mirroring.HasFlag(MirrorAxis.Horizontal))
            {
                for (int y = region.Y; y < region.Y + region.Height; y++)
                {
                    Span<TColor> buffer = topRow[y];
                    buffer.Reverse();
                    if (!topRow.IsBuffered)
                    {
                        topRow[y] = buffer;
                    }
                }
            }

            if (mirroring.HasFlag(MirrorAxis.Vertical))
            {
                // Force buffering to simply swap the rows.
                RowAccessor<TColor> bottomRow = new RowAccessor<TColor>(image, region.X, region.Width, true);

                for (int i = 0; i < region.Height / 2; i++) // Only run through half of the rows
                {
                    int topY = region.Y + i;
                    int bottomY = region.Y + region.Height - 1 - i;

                    Span<TColor> topBuffer = topRow[topY];
                    Span<TColor> bottomBuffer = bottomRow[bottomY];

                    bottomRow[bottomY] = topBuffer;
                    topRow[topY] = bottomBuffer;
                }
            }
        }

        private static void EnsureValidDimensions(ref Rectangle region)
        {
            if (region.Width < 0)
            {
                region.Width = -region.Width;
                region.X -= region.Width;
            }

            if (region.Height < 0)
            {
                region.Height = -region.Height;
                region.Y -= region.Height;
            }
        }
    }
}
