using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Helper;
using AuroraLib.Pixel.Processing.Processor;
using AuroraLib.Pixel.Processing.Resampler;
using AuroraLib.Pixel.Texture;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace AuroraLib.Pixel.Processing
{
    /// <summary>
    /// ImageExtensions
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Gets the continuous range of palette entries referenced by the image pixels.
        /// </summary>
        /// <param name="image">The palette image.</param>
        /// <param name="start">The first used palette index.</param>
        /// <param name="length">The number of palette entries from the first to the last used index.</param>
        public static void GetUsedPaletteRange(this IReadOnlyPaletteImage image, out int start, out int length)
        {
            ReadOnlySpan<int> refCounts = image.PaletteRefCounts;
#if NET8_0_OR_GREATER
            start = refCounts.IndexOfAnyExcept(0);
            int end = refCounts.LastIndexOfAnyExcept(0);
#else
            start = -1;
            int end = refCounts.Length - 1;

            for (int i = 0; i < refCounts.Length; i++)
            {
                if (refCounts[i] == 0)
                    continue;

                if (start < 0)
                    start = i;
                end = i;
            }
#endif
            length = end - start + 1;
        }

        /// <summary>
        /// Gets the number of palette entries that are referenced by the image pixels.
        /// </summary>
        /// <param name="image">The palette image.</param>
        /// <returns>The number of palette colors used by the image.</returns>
        public static int GetUsedColors(this IReadOnlyPaletteImage image)
        {
            ReadOnlySpan<int> refCounts = image.PaletteRefCounts;

#if NET8_0_OR_GREATER
            return refCounts.Length - refCounts.Count(0);
#else
            int count = 0;
            for (int i = 0; i < refCounts.Length; i++)
            {
                if (refCounts[i] != 0)
                    count++;
            }
            return count;
#endif
        }

        /// <summary>
        /// Gets the bounding rectangle of the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>
        /// A rectangle starting at (0, 0) with the width and height of the image.
        /// </returns>
        public static Rectangle GetBounds(this IReadOnlyImage image)
            => new Rectangle(0, 0, image.Width, image.Height);

        /// <summary>
        /// Clones the <paramref name="source"/> <see cref="IReadOnlyImage"/> and converts it to a new <see cref="IImage"/> of type <typeparamref name="TColor"/>.
        /// </summary>
        /// <typeparam name="TColor">The color type to which the image is cloned.</typeparam>
        /// <param name="source">The source image to clone.</param>
        /// <returns>A new <see cref="IImage{TColor}"/> that is a clone of the source image.</returns>
        public static IImage<TColor> CloneAs<TColor>(this IReadOnlyImage source) where TColor : unmanaged, IColor<TColor>
            => source.CloneAs<TColor>(source.GetBounds());

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
            if (srcRegion.Width == 0 || srcRegion.Height == 0 || target.Width == 0 || target.Height == 0)
                return;

            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(source), "Region exceeds source image bounds.");

            Rectangle targetRegion = new Rectangle(targetCoordinate, srcRegion.Size);
            Rectangle clippedTarget = Rectangle.Intersect(targetRegion, target.GetBounds());

            if (targetRegion.IsEmpty)
                return;

            if (clippedTarget != targetRegion)
            {
                srcRegion = new Rectangle(srcRegion.X + targetRegion.X - targetRegion.X, srcRegion.Y + targetRegion.Y - targetRegion.Y, targetRegion.Width, targetRegion.Height);
                targetCoordinate = targetRegion.Location;
            }


            if (target is FlatTexture<TColorT> targets && targets.LevelCount <= 1)
            {
                Size targetSize = targets.GetBounds().Size;
                for (int i = 1; i < targets.LevelCount; i++)
                {
                    IImage<TColorT> subTarget = targets.GetLevel(i);
                    Rectangle subTargetRegion = ScaleRegion(targetRegion, targetSize, subTarget.GetBounds().Size);
                    ResizeFrom(subTarget, source, srcRegion, subTargetRegion, new BoxResampler(), blendMode, intensity);
                }
            }

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
        public static void CopyFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.CopyFrom(source, source.GetBounds(), targetCoordinate, blendMode, intensity);

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.CopyFrom(source, source.GetBounds(), default, blendMode, intensity);

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, Rectangle srcRegion, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, srcRegion, targetCoordinate, blendMode, intensity));

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, source.GetBounds(), blendMode, intensity), new Rectangle(targetCoordinate, new Size(target.Width, target.Height)));

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, blendMode, intensity), target.GetBounds());

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

            if (!image.GetBounds().Contains(region))
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

        /// <summary>
        /// Resizes a region of an image using the specified resampling filter.
        /// </summary>
        /// <typeparam name="TColorT">The color type of the target image.</typeparam>
        /// <typeparam name="TColorS">The color type of the source image.</typeparam>
        /// <param name="target">The image that receives the resized result.</param>
        /// <param name="source">The source image to resize from.</param>
        /// <param name="srcRegion">The source image region to resize.</param>
        /// <param name="targetRegion">The destination region defining the output size and location.</param>
        /// <param name="resampler">The resampling filter used for interpolation.</param>
        /// <param name="blendMode">Optional blending operation used when writing to existing pixels.</param>
        /// <param name="intensity">Blend intensity used with the blend mode.</param>
        public static void ResizeFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle srcRegion, Rectangle targetRegion, IResampler resampler, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
        {
            if (srcRegion.Width <= 0 || srcRegion.Height <= 0 || targetRegion.Width <= 0 || targetRegion.Height <= 0)
                return;

            // Source regions are validated.
            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(source), "Region exceeds source image bounds.");

            // Clip the target region to the image bounds.
            targetRegion = Rectangle.Intersect(targetRegion, target.GetBounds());

            if (targetRegion.IsEmpty)
                return;

            if (target is FlatTexture<TColorT> targets && targets.LevelCount <= 1)
            {
                Size targetSize = targets.GetBounds().Size;
                for (int i = 1; i < targets.LevelCount; i++)
                {
                    IImage<TColorT> subTarget = targets.GetLevel(i);
                    Rectangle subTargetRegion = ScaleRegion(targetRegion, targetSize, subTarget.GetBounds().Size);
                    ResizeFrom(subTarget, source, srcRegion, subTargetRegion, resampler, blendMode, intensity);
                }
            }

            // Avoid unnecessary resampling when source and destination sizes match.
            if (srcRegion.Size == targetRegion.Size)
            {
                target.CopyFrom(source, srcRegion, targetRegion.Location, blendMode, intensity);
                return;
            }

            // Nearest neighbor does not require precomputed kernels.
            if (resampler is NearestNeighborResampler)
            {
                float scaleX = srcRegion.Width / (float)targetRegion.Width;
                float scaleY = srcRegion.Height / (float)targetRegion.Height;

                RowAccessor<TColorT> targetPixel = new RowAccessor<TColorT>(target, targetRegion.X, targetRegion.Width);
                ReadOnlyRowAccessor<TColorS> sourcePixel = new ReadOnlyRowAccessor<TColorS>(source, srcRegion.X, srcRegion.Width);

                for (int y = targetRegion.Top; y < targetRegion.Bottom; y++)
                {
                    Span<TColorT> targetRow = targetPixel[y];

                    int sourceY = srcRegion.Y + (int)((y - targetRegion.Y) * scaleY);
                    ReadOnlySpan<TColorS> sourceRow = sourcePixel[sourceY];

                    for (int x = targetRegion.Left; x < targetRegion.Right; x++)
                    {
                        int sourceX = (int)((x - targetRegion.X) * scaleX);

                        if (blendMode is null)
                            targetRow[x].From(sourceRow[sourceX]);
                        else
                            targetRow[x].Blend(sourceRow[sourceX], blendMode, intensity);
                    }

                    if (targetPixel.IsBuffered)
                        targetPixel[y] = targetRow;
                }

                return;
            }
            else
            {
                // Build X/Y kernels.
                using ResizeKernelMap kernelMap = new ResizeKernelMap(targetRegion.Size, srcRegion.Size, resampler);

                RowAccessor<TColorT> targetPixel = new RowAccessor<TColorT>(target, targetRegion.X, targetRegion.Width);
                ReadOnlyRowAccessor<TColorS> sourcePixel = new ReadOnlyRowAccessor<TColorS>(source, srcRegion.X, srcRegion.Width);

                for (int y = targetRegion.Top; y < targetRegion.Bottom; y++)
                {
                    Span<TColorT> targetRow = targetPixel[y];

                    Kernel kernelY = kernelMap.Y.Kernels[y - targetRegion.Y];
                    ReadOnlySpan<float> weightsY = kernelMap.Y.Weights.AsSpan(kernelY.WeightOffset, kernelY.Length);

                    for (int x = targetRegion.Left; x < targetRegion.Right; x++)
                    {
                        Kernel kernelX = kernelMap.X.Kernels[x - targetRegion.X];
                        ReadOnlySpan<float> weightsX = kernelMap.X.Weights.AsSpan(kernelX.WeightOffset, kernelX.Length);

                        Vector4 result = Vector4.Zero;
                        for (int ky = 0; ky < kernelY.Length; ky++)
                        {
                            ReadOnlySpan<TColorS> sourceRow = sourcePixel[srcRegion.Y + kernelY.Start + ky];

                            int sourceX = kernelX.Start;
                            for (int kx = 0; kx < kernelX.Length; kx++)
                            {
                                // Apply the separable filter kernel:
                                float weight = weightsY[ky] * weightsX[kx];
                                result += sourceRow[sourceX + kx].ToScaledVector4() * weight;
                            }
                        }

                        // Some filters (for example Lanczos) can produce values outside the valid color range.
                        result = Vector4.Clamp(result, Vector4.Zero, Vector4.One);

                        if (blendMode is null)
                            targetRow[x].FromScaledVector4(result);
                        else
                            targetRow[x].FromScaledVector4(blendMode(targetRow[x].ToScaledVector4(), result, intensity));
                    }

                    // Write the processed row back when using a buffered accessor.
                    if (targetPixel.IsBuffered)
                        targetPixel[y] = targetRow;
                }
            }
        }
        private static Rectangle ScaleRegion(Rectangle region, Size from, Size to)
            => new Rectangle(region.X * to.Width / from.Width, region.Y * to.Height / from.Height, region.Width * to.Width / from.Width, region.Height * to.Height / from.Height);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, IResampler resampler, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.ResizeFrom(source, source.GetBounds(), target.GetBounds(), resampler, blendMode, intensity);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom(this IImage target, IReadOnlyImage source, Rectangle srcRegion, Rectangle targetRegion, IResampler resampler, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new ResizeProcessor(source, srcRegion, resampler, blendMode, intensity), targetRegion);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom(this IImage target, IReadOnlyImage source, IResampler resampler, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new ResizeProcessor(source, source.GetBounds(), resampler, blendMode, intensity), target.GetBounds());



        /// <inheritdoc cref="IReadOnlyImage.Apply(IReadOnlyPixelProcessor, Rectangle)"/>
        public static void Apply(this IReadOnlyImage image, IReadOnlyPixelProcessor processor)
            => image.Apply(processor, image.GetBounds());

        /// <inheritdoc cref="IImage.Apply(IPixelProcessor, Rectangle)"/>
        public static void Apply(this IImage image, IPixelProcessor processor)
            => image.Apply(processor, image.GetBounds());
    }
}
