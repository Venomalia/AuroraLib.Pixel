using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Analyzer;
using AuroraLib.Pixel.Processing.Helper;
using AuroraLib.Pixel.Processing.Processor;
using AuroraLib.Pixel.Processing.Resampler;
using AuroraLib.Pixel.Texture;
using System;
using System.Buffers;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        public static void CopyFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle srcRegion, Point targetCoordinate = default, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT>
            where TColorS : unmanaged, IColor<TColorS>
        {
            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(srcRegion), "Region exceeds source image bounds.");

            Rectangle targetRegion = new Rectangle(targetCoordinate, srcRegion.Size);
            ClipRegions(target.GetBounds(), ref srcRegion, ref targetRegion);
            targetCoordinate = targetRegion.Location;

            if (srcRegion.Width == 0 || srcRegion.Height == 0)
                return;

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

            if (ReferenceEquals(source, target) && srcRegion.IntersectsWith(targetRegion) && srcRegion.X >= targetCoordinate.X)
            {
                using var buffer = new MemoryImage<TColorT>(srcRegion.Width, srcRegion.Height);
                buffer.CopyFrom(source, srcRegion);
                CopyFrom(target, buffer, targetRegion, targetCoordinate, blendMode, intensity);
                return;
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

        private static void ClipRegions(Rectangle targetBounds, ref Rectangle srcRegion, ref Rectangle targetRegion)
        {
            Rectangle clipped = Rectangle.Intersect(targetRegion, targetBounds);

            if (clipped != targetRegion)
            {
                srcRegion = new Rectangle(
                    srcRegion.X + clipped.X - targetRegion.X,
                    srcRegion.Y + clipped.Y - targetRegion.Y,
                    clipped.Width,
                    clipped.Height);

                targetRegion = clipped;
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
            => target.Apply(new CopyRegionProcessor(source, srcRegion, blendMode, intensity), new Rectangle(targetCoordinate, new Size(source.Width, source.Height)));

        /// <inheritdoc cref="CopyFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Point, BlendModes.BlendFunction?, float)"/>
        public static void CopyFrom(this IImage target, IReadOnlyImage source, Point targetCoordinate, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new CopyRegionProcessor(source, source.GetBounds(), blendMode, intensity), new Rectangle(targetCoordinate, new Size(source.Width, source.Height)));

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

            if (image is Texture<TColor> tex && tex.LevelCount <= 1)
            {
                Size targetSize = tex.GetBounds().Size;
                for (int i = 1; i < tex.LevelCount; i++)
                {
                    IImage<TColor> subTarget = tex.GetLevel(i);
                    Rectangle subTargetRegion = ScaleRegion(region, targetSize, subTarget.GetBounds().Size);
                    Mirror(subTarget, mirroring, subTargetRegion);
                }
            }

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
        public static void ResizeFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle srcRegion, Rectangle targetRegion, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
        {
            resampler ??= Resamplers.Default;

            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(srcRegion), "Region exceeds source image bounds.");

            ClipResizeRegions(target.GetBounds(), ref srcRegion, ref targetRegion);

            if (srcRegion.Width == 0 || srcRegion.Height == 0 || targetRegion.Width == 0 || targetRegion.Height == 0)
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

            if (ReferenceEquals(source, target) && srcRegion.IntersectsWith(targetRegion))
            {
                using var buffer = new MemoryImage<TColorT>(srcRegion.Width, srcRegion.Height);
                buffer.CopyFrom(source, srcRegion);
                ResizeFrom(target, buffer, targetRegion, resampler, blendMode, intensity);
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

                    for (int x = 0; x < targetRegion.Width; x++)
                    {
                        int sourceX = (int)(x * scaleX);

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

                    for (int x = 0; x < targetRegion.Width; x++)
                    {
                        Kernel kernelX = kernelMap.X.Kernels[x];
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

        private static void ClipResizeRegions(Rectangle targetBounds, ref Rectangle srcRegion, ref Rectangle targetRegion)
        {
            Rectangle clipped = Rectangle.Intersect(targetRegion, targetBounds);

            if (clipped != targetRegion)
            {
                float scaleX = srcRegion.Width / (float)targetRegion.Width;
                float scaleY = srcRegion.Height / (float)targetRegion.Height;

                int offsetX = (int)((clipped.X - targetRegion.X) * scaleX);
                int offsetY = (int)((clipped.Y - targetRegion.Y) * scaleY);

                int width = (int)(clipped.Width * scaleX);
                int height = (int)(clipped.Height * scaleY);

                srcRegion = new Rectangle(srcRegion.X + offsetX, srcRegion.Y + offsetY, width, height);
                targetRegion = clipped;
            }
        }

        private static Rectangle ScaleRegion(Rectangle region, Size from, Size to)
            => new Rectangle(region.X * to.Width / from.Width, region.Y * to.Height / from.Height, region.Width * to.Width / from.Width, region.Height * to.Height / from.Height);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.ResizeFrom(source, source.GetBounds(), target.GetBounds(), resampler, blendMode, intensity);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom(this IImage target, IReadOnlyImage source, Rectangle srcRegion, Rectangle targetRegion, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new ResizeProcessor(source, srcRegion, resampler, blendMode, intensity), targetRegion);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom(this IImage target, IReadOnlyImage source, Rectangle targetRegion, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new ResizeProcessor(source, source.GetBounds(), resampler, blendMode, intensity), targetRegion);

        /// <inheritdoc cref="ResizeFrom{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, IResampler, BlendModes.BlendFunction?, float)"/>
        public static void ResizeFrom(this IImage target, IReadOnlyImage source, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new ResizeProcessor(source, source.GetBounds(), resampler, blendMode, intensity), target.GetBounds());

        /// <summary>
        /// Resizes a region of the source image to the specified size.
        /// </summary>
        /// <param name="source">The source image to resize.</param>
        /// <param name="srcRegion">The region of the source image to resize.</param>
        /// <param name="size">The size of the resulting image.</param>
        /// <param name="resampler">The resampler used to resize the image, or <see langword="null"/> to use the default resampler.</param>
        /// <returns>A new image containing the resized region.</returns>
        public static IImage Resize(this IReadOnlyImage source, Rectangle srcRegion, Size size, IResampler? resampler = null)
        {
            IImage target = source.Create(size.Width, size.Height);
            target.ResizeFrom(source, srcRegion, target.GetBounds(), resampler);
            return target;
        }

        /// <inheritdoc cref="Resize(IReadOnlyImage, Rectangle, Size, IResampler?)"/>
        public static IImage Resize(this IReadOnlyImage source, Size size, IResampler? resampler = null)
            => Resize(source, source.GetBounds(), size, resampler);


        /// <inheritdoc cref="IReadOnlyImage.Apply(IReadOnlyPixelProcessor, Rectangle)"/>
        public static void Apply(this IReadOnlyImage image, IReadOnlyPixelProcessor processor)
            => image.Apply(processor, image.GetBounds());

        /// <inheritdoc cref="IImage.Apply(IPixelProcessor, Rectangle)"/>
        public static void Apply(this IImage image, IPixelProcessor processor)
            => image.Apply(processor, image.GetBounds());

        /// <summary>
        /// Analyzes the specified image region using the provided analyzer.
        /// </summary>
        /// <typeparam name="TResult">The type of result produced by the analyzer.</typeparam>
        /// <param name="image">The image to analyze.</param>
        /// <param name="analyzer">The analyzer to use.</param>
        /// <param name="region">The region of the image to analyze.</param>
        /// <returns>The result of the analysis.</returns>
        public static TResult Apply<TResult>(this IReadOnlyImage image, IAnalyzer<TResult> analyzer, Rectangle region)
        {
            var processor = new AnalyzerProcessor<TResult>(analyzer);
            image.Apply(processor, region);
            return processor.Result;
        }

        /// <inheritdoc cref="Apply{TResult}(IReadOnlyImage, IAnalyzer{TResult}, Rectangle)"/>
        public static TResult Apply<TResult>(this IReadOnlyImage image, IAnalyzer<TResult> analyzer)
            => Apply(image, analyzer, image.GetBounds());

        /// <summary>
        /// Processes a span of image pixels.
        /// </summary>
        /// <typeparam name="TColor">The pixel color type.</typeparam>
        /// <param name="pixels">The pixels to process.</param>
        public delegate void PixelProcessor<TColor>(Span<TColor> pixels) where TColor : unmanaged, IColor<TColor>;

        /// <summary>
        /// Applies a pixel operation to an image.
        /// </summary>
        /// <typeparam name="TColor">The pixel color type.</typeparam>
        /// <param name="image">The image to process.</param>
        /// <param name="operation">The operation to apply to the pixels.</param>
        /// <param name="region">The region of the image to process.</param>
        public static void Apply<TColor>(this IImage<TColor> image, PixelProcessor<TColor> operation, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            bool all = region == image.GetBounds();
            if (all && image is IPaletteImage<TColor> p)
            {
                p.GetUsedPaletteRange(out int start, out int length);
                operation(p.Palette.Span.Slice(start, length));
            }
            else if (image is Texture<TColor> tex)
            {
                Size targetSize = image.GetBounds().Size;
                foreach (var level in tex)
                {
                    Rectangle mipRegion = ScaleRegion(region, targetSize, level.GetBounds().Size);
                    level.Apply(operation, mipRegion);
                }
            }
            else if (all && image is MemoryImage<TColor> mImage && mImage.Width == mImage.Stride)
            {
                operation(mImage.Pixel);
            }
            else if (image is IDirectRowAccess<TColor> rowAccess)
            {
                for (int y = region.Y; y < region.Bottom; y++)
                    operation(rowAccess.GetWritableRow(y).Slice(region.X, region.Width));
            }
            else
            {
                const int StackallocThreshold = 4096;
                int rowBytes = region.Width * Unsafe.SizeOf<TColor>();
                byte[]? buffer = rowBytes > StackallocThreshold ? ArrayPool<byte>.Shared.Rent(rowBytes) : null;
                Span<TColor> row = buffer == null ? stackalloc TColor[region.Width] : MemoryMarshal.Cast<byte, TColor>(buffer.AsSpan(0, rowBytes));

                try
                {
                    for (int y = region.Y; y < region.Bottom; y++)
                    {
                        image.GetPixel(region.X, y, row);
                        operation(row);
                        image.SetPixel(region.X, y, row);
                    }
                }
                finally
                {
                    if (buffer != null)
                        ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        /// <inheritdoc cref="Apply{TColor}(IImage{TColor}, PixelProcessor{TColor}, Rectangle)"/>
        public static void Apply<TColor>(this IImage<TColor> image, PixelProcessor<TColor> operation) where TColor : unmanaged, IColor<TColor>
            => Apply(image, operation, image.GetBounds());

        /// <summary>
        /// Applies a 2D affine transformation to a region of a source image and draws the result into a target region.
        /// </summary>
        /// <typeparam name="TColorT">The target image color type.</typeparam>
        /// <typeparam name="TColorS">The source image color type.</typeparam>
        /// <param name="target">The image to draw the transformed region into.</param>
        /// <param name="source">The image containing the source region.</param>
        /// <param name="srcRegion">The region of the source image to transform.</param>
        /// <param name="targetRegion">The region of the target image to draw into.</param>
        /// <param name="transform">The affine transformation matrix mapping source coordinates to target coordinates.</param>
        /// <param name="resampler">The resampler used to interpolate source pixels.</param>
        /// <param name="blendMode">The blend mode used to combine the transformed pixels with the target.</param>
        /// <param name="intensity">The intensity of the blend operation.</param>
        public static void Transform<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle srcRegion, Rectangle targetRegion, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f) where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
        {
            resampler ??= Resamplers.Default;

            if (!source.GetBounds().Contains(srcRegion))
                throw new ArgumentOutOfRangeException(nameof(srcRegion), "Region exceeds source image bounds.");

            if (transform.IsIdentity)
            {
                var size = new Size(Math.Min(targetRegion.Width, srcRegion.Width), Math.Min(targetRegion.Height, srcRegion.Height));
                target.CopyFrom(source, new Rectangle(srcRegion.Location, size), targetRegion.Location, blendMode, intensity);
                return;
            }

            targetRegion = Rectangle.Intersect(targetRegion, TransformBounds(srcRegion, transform));
            targetRegion = Rectangle.Intersect(targetRegion, target.GetBounds());

            if (srcRegion.Width == 0 || srcRegion.Height == 0 || targetRegion.Width == 0 || targetRegion.Height == 0)
                return;

            if (!Matrix3x2.Invert(transform, out Matrix3x2 inverse))
                return;

            if (ReferenceEquals(source, target) && srcRegion.IntersectsWith(targetRegion))
            {
                using var buffer = new MemoryImage<TColorT>(srcRegion.Width, srcRegion.Height);
                buffer.CopyFrom(source, srcRegion);

                transform = Matrix3x2.CreateTranslation(srcRegion.X, srcRegion.Y) * transform;
                Transform(target, buffer, targetRegion, transform, resampler, blendMode, intensity);
                return;
            }

            RowAccessor<TColorT> targetPixel = new RowAccessor<TColorT>(target, targetRegion.X, targetRegion.Width);
            if (resampler is NearestNeighborResampler)
            {
                for (int y = targetRegion.Top; y < targetRegion.Bottom; y++)
                {
                    Span<TColorT> targetRow = targetPixel[y];

                    for (int x = targetRegion.Left; x < targetRegion.Right; x++)
                    {
                        Vector2 targetPos = new Vector2(x + 0.5f, y + 0.5f);
                        Vector2 sourcePos = Vector2.Transform(targetPos, inverse);
                        sourcePos -= new Vector2(0.5f);

                        int sourceX = Floor(sourcePos.X + 0.5f);
                        int sourceY = Floor(sourcePos.Y + 0.5f);

                        if ((uint)(sourceX - srcRegion.X) >= (uint)srcRegion.Width || (uint)(sourceY - srcRegion.Y) >= (uint)srcRegion.Height)
                            continue;

                        TColorS sourceColor = source[sourceX, sourceY];

                        if (blendMode is null)
                            targetRow[x - targetRegion.X].From(sourceColor);
                        else
                            targetRow[x - targetRegion.X].Blend(sourceColor, blendMode, intensity);
                    }

                    if (targetPixel.IsBuffered)
                        targetPixel[y] = targetRow;
                }
            }
            else
            {
                float radius = resampler.Radius;
                ReadOnlyRowAccessor<TColorS> sourcePixel = new ReadOnlyRowAccessor<TColorS>(source, srcRegion.X, srcRegion.Width);
                for (int y = targetRegion.Top; y < targetRegion.Bottom; y++)
                {
                    Span<TColorT> targetRow = targetPixel[y];

                    for (int x = targetRegion.Left; x < targetRegion.Right; x++)
                    {
                        // Target pixel center.
                        Vector2 targetPos = new Vector2(x + 0.5f, y + 0.5f);

                        // Find where this target pixel came from in the source.
                        Vector2 sourcePos = Vector2.Transform(targetPos, inverse);

                        // Convert back to source pixel-center coordinates.
                        sourcePos -= new Vector2(0.5f);
                        int startX = Math.Max(srcRegion.X, Ceiling(sourcePos.X - radius));
                        int endX = Math.Min(srcRegion.Right - 1, Floor(sourcePos.X + radius));

                        int startY = Math.Max(srcRegion.Y, Ceiling(sourcePos.Y - radius));
                        int endY = Math.Min(srcRegion.Bottom - 1, Floor(sourcePos.Y + radius));

                        Vector4 result = Vector4.Zero;
                        float weightSum = 0;

                        for (int sy = startY; sy <= endY; sy++)
                        {
                            float weightY = resampler.GetWeight(sy - sourcePos.Y);
                            ReadOnlySpan<TColorS> sourceRow = sourcePixel[sy];

                            for (int sx = startX; sx <= endX; sx++)
                            {
                                float weight = resampler.GetWeight(sx - sourcePos.X) * weightY;

                                result += sourceRow[sx].ToScaledVector4() * weight;
                                weightSum += weight;
                            }
                        }

                        if (weightSum <= 0)
                            continue;

                        result /= weightSum;

                        result = Vector4.Clamp(result, Vector4.Zero, Vector4.One);

                        if (blendMode is null)
                            targetRow[x - targetRegion.X].FromScaledVector4(result);
                        else
                            targetRow[x - targetRegion.X].FromScaledVector4(blendMode(targetRow[x - targetRegion.X].ToScaledVector4(), result, intensity));
                    }

                    if (targetPixel.IsBuffered)
                        targetPixel[y] = targetRow;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Ceiling(float x)
#if NET6_0_OR_GREATER
            => (int)MathF.Ceiling(x);
#else
            => (int)Math.Ceiling(x);
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Floor(float x)
#if NET6_0_OR_GREATER
            => (int)MathF.Floor(x);
#else
            => (int)Math.Floor(x);
#endif

        private static Rectangle TransformBounds(Rectangle region, Matrix3x2 transform)
        {
            Vector2 p1 = Vector2.Transform(new Vector2(region.Left, region.Top), transform);
            Vector2 p2 = Vector2.Transform(new Vector2(region.Right, region.Top), transform);
            Vector2 p3 = Vector2.Transform(new Vector2(region.Left, region.Bottom), transform);
            Vector2 p4 = Vector2.Transform(new Vector2(region.Right, region.Bottom), transform);
            
            float left = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
            float top = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));

            float right = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
            float bottom = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

            return Rectangle.FromLTRB(Floor(left), Floor(top), Ceiling(right), Ceiling(bottom));
        }

        /// <inheritdoc cref="Transform{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, Matrix3x2, IResampler?, BlendModes.BlendFunction?, float)"/>
        public static void Transform<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Rectangle targetRegion, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.Transform(source, source.GetBounds(), targetRegion, transform, resampler, blendMode, intensity);

        /// <inheritdoc cref="Transform{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, Matrix3x2, IResampler?, BlendModes.BlendFunction?, float)"/>
        public static void Transform<TColorT, TColorS>(this IImage<TColorT> target, IReadOnlyImage<TColorS> source, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            where TColorT : unmanaged, IColor<TColorT> where TColorS : unmanaged, IColor<TColorS>
            => target.Transform(source, source.GetBounds(), target.GetBounds(), transform, resampler, blendMode, intensity);

        /// <inheritdoc cref="Transform{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, Matrix3x2, IResampler?, BlendModes.BlendFunction?, float)"/>
        public static void Transform(this IImage target, IReadOnlyImage source, Rectangle srcRegion, Rectangle targetRegion, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new TransformationProcessor(source, srcRegion, transform, resampler, blendMode, intensity), targetRegion);

        /// <inheritdoc cref="Transform{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, Matrix3x2, IResampler?, BlendModes.BlendFunction?, float)"/>
        public static void Transform(this IImage target, IReadOnlyImage source, Rectangle targetRegion, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new TransformationProcessor(source, source.GetBounds(), transform, resampler, blendMode, intensity), targetRegion);

        /// <inheritdoc cref="Transform{TColorT, TColorS}(IImage{TColorT}, IReadOnlyImage{TColorS}, Rectangle, Rectangle, Matrix3x2, IResampler?, BlendModes.BlendFunction?, float)"/>
        public static void Transform(this IImage target, IReadOnlyImage source, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? blendMode = null, float intensity = 1f)
            => target.Apply(new TransformationProcessor(source, source.GetBounds(), transform, resampler, blendMode, intensity), target.GetBounds());
    }
}
