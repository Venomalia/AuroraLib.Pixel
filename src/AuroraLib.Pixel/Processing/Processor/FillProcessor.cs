using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using System;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Processes an image by filling a region with a specified color.
    /// </summary>
    public sealed class FillProcessor : IPixelProcessor
    {
        /// <summary>
        /// Gets or sets the color used to fill the region.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the optional blend function used when filling the region.
        /// </summary>
        public BlendModes.BlendFunction? BlendFunction { get; set; }

        public FillProcessor(Vector4 color, BlendModes.BlendFunction? blendMode = null)
        {
            Color = color;
            BlendFunction = blendMode;
        }

        /// <inheritdoc/>
        public void Apply<TColor>(IImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            if (BlendFunction == null)
                image.Apply(Fill, region);
            else
                image.Apply(BlendFill, region);
        }

        private void Fill<TColor>(Span<TColor> pixels) where TColor : unmanaged, IColor<TColor>
        {
            TColor color = default;
            color.FromScaledVector4(Color);
            pixels.Fill(color);
        }

        private void BlendFill<TColor>(Span<TColor> pixels)
            where TColor : unmanaged, IColor<TColor>
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                ref TColor pixel = ref pixels[i];
                pixel.FromScaledVector4(BlendFunction!(pixel.ToScaledVector4(), Color, 1));
            }
        }
    }
}
