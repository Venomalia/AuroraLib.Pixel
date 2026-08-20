using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.PixelFormats;
using System;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    /// <summary>
    /// Analyzes an image to determine its transparency mode.
    /// </summary>
    public sealed class TransparencyAnalyzer : Analyzer<TransparencyMode>
    {
        /// <inheritdoc/>
        public override TransparencyMode Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region)
        {
            var format = default(TColor).FormatInfo;
            if (!format.HasAlpha)
                return TransparencyMode.Opaque;

            var result = Analyze(image, region, TransparencyMode.Opaque);

            if (result == TransparencyMode.Straight && image.Metadata?.SamplingInfos?.TransparencyMode == TransparencyMode.Premultiplied)
                return TransparencyMode.Premultiplied;
            return result;
        }

        protected override bool Analyze<TColor>(ReadOnlySpan<TColor> pixels, ref TransparencyMode state)
        {
            RGBA<byte> rgba = default;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].ToRGBA(ref rgba);

                switch (rgba.A)
                {
                    case byte.MaxValue:
                        break;

                    case byte.MinValue:
                        state = TransparencyMode.Cutout;
                        break;

                    default:
                        state = TransparencyMode.Straight;
                        return true;
                }
            }

            return false;
        }
    }
}
