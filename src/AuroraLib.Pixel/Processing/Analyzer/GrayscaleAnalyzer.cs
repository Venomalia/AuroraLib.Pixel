using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using System;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    /// <summary>
    /// Analyzes an image to determine whether it contains color information.
    /// </summary>
    public sealed class GrayscaleAnalyzer : Analyzer<bool>
    {
        /// <summary>
        /// Gets or sets the maximum allowed difference between color channels for a pixel to be considered grayscale.
        /// </summary>
        public byte Tolerance { get; set; }

        public GrayscaleAnalyzer(byte tolerance = 3) 
            => Tolerance = tolerance;

        /// <inheritdoc/>
        public override bool Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region)
        {
            if (default(TColor).FormatInfo.IsGrayscale)
                return true;

            return Analyze(image, region, false);
        }


        /// <inheritdoc/>
        protected override bool Analyze<TColor>(ReadOnlySpan<TColor> pixels, ref bool state)
        {
            RGBA<byte> rgba = default;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].ToRGBA(ref rgba);
                int maxabs = Math.Max(Math.Abs(rgba.R - rgba.G), Math.Abs(rgba.R - rgba.B));
                if (maxabs > Tolerance)
                {
                    state = true;
                    return true;
                }
            }

            return false;
        }
    }
}
