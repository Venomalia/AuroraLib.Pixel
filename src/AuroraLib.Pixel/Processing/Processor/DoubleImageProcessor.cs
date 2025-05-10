using AuroraLib.Pixel.Image;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Represents an abstract base class for image processors that operate on two images simultaneously.
    /// </summary>
    public abstract class DoubleImageProcessor : IPixelProcessor
    {
        /// <summary>
        /// The source image from which data is read during processing.
        /// </summary>
        protected IReadOnlyImage SourceImage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleImageProcessor"/> class with the specified source image.
        /// </summary>
        /// <param name="sourceImage">The source image used as input during processing.</param>
        protected DoubleImageProcessor(IReadOnlyImage sourceImage)
            => SourceImage = sourceImage;

        /// <inheritdoc/>
        public void Apply<TColor>(IImage<TColor> target) where TColor : unmanaged, IColor<TColor>
            => SourceImage.Apply(new PixelProcessorActivator<TColor>(target, this));

        /// <summary>
        /// Applies the image processing operation using the specified source and target images.
        /// </summary>
        /// <typeparam name="TColor1">The pixel type of the target image.</typeparam>
        /// <typeparam name="TColor2">The pixel type of the source image.</typeparam>
        /// <param name="target">The image to be modified.</param>
        /// <param name="source">The source image providing input data.</param>
        protected abstract void Apply<TColor1, TColor2>(IImage<TColor1> target, IReadOnlyImage<TColor2> source)
            where TColor1 : unmanaged, IColor<TColor1> where TColor2 : unmanaged, IColor<TColor2>;

        private sealed class PixelProcessorActivator<TColor1> : IReadOnlyPixelProcessor where TColor1 : unmanaged, IColor<TColor1>
        {
            private readonly IImage<TColor1> _target;
            private readonly DoubleImageProcessor _context;

            public PixelProcessorActivator(IImage<TColor1> targetImage, DoubleImageProcessor context)
            {
                _target = targetImage;
                _context = context;
            }

            public void Apply<TColor>(IReadOnlyImage<TColor> image) where TColor : unmanaged, IColor<TColor>
                => _context.Apply(_target, image);
        }
    }
}