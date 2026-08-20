using AuroraLib.Pixel.Image;
using System;
using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    /// <summary>
    /// Provides a base class for analyzing image pixel data and producing a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result produced by the analyzer.</typeparam>
    public abstract class Analyzer<TResult>
    {
        /// <summary>
        /// Analyzes the specified image region using the default analysis state.
        /// </summary>
        /// <typeparam name="TColor">The pixel color type of the image.</typeparam>
        /// <param name="image">The image to analyze.</param>
        /// <param name="region">The region of the image to analyze.</param>
        /// <returns>The result of the analysis.</returns>
        public virtual TResult Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>
            => Analyze(image, region, default);

        /// <summary>
        /// Analyzes the specified image region using the supplied initial state.
        /// </summary>
        /// <typeparam name="TColor">The pixel color type of the image.</typeparam>
        /// <param name="image">The image to analyze.</param>
        /// <param name="region">The region of the image to analyze.</param>
        /// <param name="state">The initial state used during the analysis.</param>
        /// <returns>The final analysis result.</returns>
        protected virtual TResult Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region, TResult state) where TColor : unmanaged, IColor<TColor>
        {
            bool all = region == image.GetBounds();
            if (all && image is IReadOnlyPaletteImage<TColor> p)
            {
                p.GetUsedPaletteRange(out int start, out int length);
                Analyze(p.Palette.Slice(start, length), ref state);
            }
            else if (all && image is MemoryImage<TColor> mImage && mImage.Width == mImage.Stride)
            {
                Analyze<TColor>(mImage.Pixel, ref state);
            }
            else if (image is IReadOnlyDirectRowAccess<TColor> rowAccess)
            {
                for (int y = region.Y; y < region.Bottom; y++)

                    if (Analyze(rowAccess.GetRow(y).Slice(region.X, region.Width), ref state))
                        return state;
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
                        if (Analyze<TColor>(row, ref state))
                            return state;
                    }
                }
                finally
                {
                    if (buffer != null)
                        ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            return state;
        }

        /// <summary>
        /// Analyzes the specified pixels and updates the analysis state.
        /// </summary>
        /// <typeparam name="TColor">The pixel color type.</typeparam>
        /// <param name="pixels">The pixels to analyze.</param>
        /// <param name="state">The current analysis state.</param>
        /// <returns><see langword="true"/> to stop the analysis early; otherwise, <see langword="false"/>.</returns>
        protected abstract bool Analyze<TColor>(ReadOnlySpan<TColor> pixels, ref TResult state) where TColor : unmanaged, IColor<TColor>;
    }
}
