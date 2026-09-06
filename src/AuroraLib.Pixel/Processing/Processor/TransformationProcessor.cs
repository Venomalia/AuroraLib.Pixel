using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.Processing.Resampler;
using System;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Applies an affine transformation to an image.
    /// </summary>
    public sealed class TransformationProcessor : DoubleImageProcessor
    {
        /// <summary>
        /// Gets or sets the affine transformation matrix.
        /// </summary>
        public Matrix3x2 Transformation { get; set; }

        /// <summary>
        /// The resampling filter used to calculate the resized pixels.
        /// </summary>
        public IResampler Resampler { get; set; }

        /// <summary>
        /// The blend mode to be applied during the copy operation. If <c>null</c>, no blending is applied.
        /// </summary>
        public BlendModes.BlendFunction? BlendMode { get; set; }

        /// <summary>
        /// The intensity of the blending.
        /// </summary>
        public float Intensity { get; set; }

        public TransformationProcessor(IReadOnlyImage sourceImage, Rectangle region, Matrix3x2 transform, IResampler? resampler = null, BlendModes.BlendFunction? mode = null, float intensity = 1f) : base(sourceImage, region)
        {
            Transformation = transform;
            Resampler = resampler ?? Resamplers.Default;
            BlendMode = mode;
            Intensity = intensity;
        }

        public TransformationProcessor(IReadOnlyImage sourceImage, Rectangle region, float scale, float rotationDegrees = 0, IResampler? resampler = null, BlendModes.BlendFunction? mode = null, float intensity = 1f) : this(sourceImage, region, CreateTransform(scale, rotationDegrees, new Vector2(region.Width / 2f, region.Height / 2f)), resampler, mode, intensity)
        { }

        private static Matrix3x2 CreateTransform(float scale, float rotationDegrees, Vector2 center)
        {
            Matrix3x2 transform = Matrix3x2.CreateScale(scale, center);
            transform *= Matrix3x2.CreateRotation(rotationDegrees * ((float)Math.PI / 180f), center);
            return transform;
        }

        protected override void Apply<TColor1, TColor2>(IImage<TColor1> target, IReadOnlyImage<TColor2> source, Rectangle targetRegion, Rectangle sourceRegion)
            => target.Transform(source, sourceRegion, targetRegion, Transformation, Resampler, BlendMode, Intensity);
    }
}
