using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.Processing.Resampler;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{

    /// <summary>
    /// Resizes a region of the source image into a target region using the specified resampling filter.
    /// </summary>
    public sealed class ResizeProcessor : DoubleImageProcessor
    {
        /// <summary>
        /// The region of the source image to resize.
        /// </summary>
        private Rectangle SrcRegion { get; set; }

        /// <summary>
        /// The destination region where the resized image is written.
        /// </summary>
        public Rectangle TargetRegion { get; set; }

        /// <summary>
        /// The resampling filter used to calculate the resized pixels.
        /// </summary>
        public IResampler Resampler { get; set; }

        /// <summary>
        /// The blend mode to apply when writing the resized pixels. If <c>null</c>, the pixels replace the existing target pixels.
        /// </summary>
        public BlendModes.BlendFunction? BlendMode { get; set; }

        /// <summary>
        /// The intensity of the blending operation.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeProcessor"/> class.
        /// </summary>
        /// <param name="source">The source image to resize from.</param>
        /// <param name="srcRegion">The region of the source image to resize.</param>
        /// <param name="targetRegion">The destination region defining the output size and location.</param>
        /// <param name="resampler">The resampling filter used for interpolation.</param>
        /// <param name="blendMode">The optional blend mode to apply to the resized pixels.</param>
        /// <param name="intensity">The intensity of the blending operation.</param>
        public ResizeProcessor(IReadOnlyImage source, Rectangle srcRegion, Rectangle targetRegion, IResampler resampler, BlendModes.BlendFunction? blendMode = null, float intensity = 1f) : base(source)
        {
            SrcRegion = srcRegion;
            TargetRegion = targetRegion;
            Resampler = resampler;
            BlendMode = blendMode;
            Intensity = intensity;
        }

        /// <inheritdoc/>
        protected override void Apply<TColorT, TColorS>(IImage<TColorT> target, IReadOnlyImage<TColorS> source)
            => target.ResizeFrom(source, SrcRegion, TargetRegion, Resampler, BlendMode, Intensity);
    }
}
