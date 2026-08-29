using AuroraLib.Pixel.Image;
using System;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    /// <summary>
    /// Analyzes an image to determine the distribution of its channel values.
    /// </summary>
    public sealed class HistogramAnalyzer : Analyzer<Histogram>
    {
        /// <inheritdoc/>
        public override Histogram Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region)
        {
            Histogram histogram = new Histogram();
            try
            {
                return Analyze(image, region, histogram);
            }
            catch
            {
                histogram.Dispose();
                throw;
            }
        }

        protected override bool Analyze<TColor>(ReadOnlySpan<TColor> pixels, ref Histogram state)
        {
            state.Add(pixels);
            return false;
        }
    }
}
