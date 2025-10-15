namespace AuroraLib.Pixel
{
    /// <summary>
    /// Describes the layout of color channels in a pixel format.
    /// </summary>
    public readonly struct PixelFormatInfo
    {
        private readonly byte RedDepth, GreenDepth, BlueDepth, AlphaDepth;

        private readonly byte RedMaskShift, GreenMaskShift, BlueMaskShift, AlphaMaskShift;

        /// <summary>
        /// Total number of bits used to represent a single pixel.
        /// </summary>
        public readonly byte BitsPerPixel;

        /// <summary>
        /// The data type used by all channels in this pixel format.
        /// </summary>
        public readonly ChannelType Type;

        /// <summary>
        /// The color space of this pixel.
        /// </summary>
        public readonly ColorSpaceType ColorSpace;

        /// <summary>
        /// Info for red channel.
        /// </summary>
        public readonly ChannelInfo RedChannelInfo => new ChannelInfo(RedDepth, RedMaskShift);

        /// <summary>
        /// Info for green channel.
        /// </summary>
        public readonly ChannelInfo GreenChannelInfo => new ChannelInfo(GreenDepth, GreenMaskShift);

        /// <summary>
        /// Info for blue channel.
        /// </summary>
        public readonly ChannelInfo BlueChannelInfo => new ChannelInfo(BlueDepth, BlueMaskShift);

        /// <summary>
        /// Info for alpha channel.
        /// </summary>
        public readonly ChannelInfo AlphaChannelInfo => new ChannelInfo(AlphaDepth, AlphaMaskShift);

        /// <summary>
        /// Indicates whether the pixel format includes an alpha channel.
        /// </summary>
        public readonly bool HasAlpha => AlphaDepth != 0;

        /// <summary>
        /// Indicates whether the pixel format includes an color channel.
        /// </summary>
        public readonly bool HasColor => RedDepth != 0 || GreenDepth != 0 || BlueDepth != 0;

        /// <summary>
        /// Indicates whether the format is grayscale.
        /// </summary>
        public readonly bool IsGrayscale => RedMaskShift == GreenMaskShift && RedMaskShift == BlueMaskShift;

        public PixelFormatInfo(byte bitsPerPixel, byte redDepth, byte redMaskShift, byte greenDepth, byte greenMaskShift, byte blueDepth, byte blueMaskShift, byte alphaDepth = 0, byte alphaMaskShift = 0, ColorSpaceType colorSpace = default, ChannelType type = default)
        {
            BitsPerPixel = bitsPerPixel;
            RedDepth = redDepth;
            GreenDepth = greenDepth;
            BlueDepth = blueDepth;
            AlphaDepth = alphaDepth;
            RedMaskShift = redMaskShift;
            GreenMaskShift = greenMaskShift;
            BlueMaskShift = blueMaskShift;
            AlphaMaskShift = alphaMaskShift;
            ColorSpace = colorSpace;
            Type = type;
        }

        public PixelFormatInfo(byte bitsPerPixel, ulong redMask, ulong greenMask, ulong blueMask, ulong alphaMask = 0, ColorSpaceType colorSpace = default, ChannelType type = default)
        {
            BitsPerPixel = bitsPerPixel;
            ChannelInfo red = new ChannelInfo(redMask);
            RedDepth = red.BitDepth;
            RedMaskShift = red.Shift;
            ChannelInfo green = new ChannelInfo(greenMask);
            GreenDepth = green.BitDepth;
            GreenMaskShift = green.Shift;
            ChannelInfo blue = new ChannelInfo(blueMask);
            BlueDepth = blue.BitDepth;
            BlueMaskShift = blue.Shift;
            ChannelInfo alpha = new ChannelInfo(alphaMask);
            AlphaDepth = alpha.BitDepth;
            AlphaMaskShift = alpha.Shift;
            ColorSpace = colorSpace;
            Type = type;
        }

        /// <summary>
        /// Specifies the numeric type of a color channel.
        /// </summary>
        public enum ChannelType : byte
        {
            /// <summary>Unsigned integer channel.</summary>
            Unsigned = default,
            /// <summary>Signed integer channel.</summary>
            Signed,
            /// <summary>Floating-point channel.</summary>
            Float,
            /// <summary>
            /// Channel type determined at runtime; both type and bit depth may vary.
            /// </summary>
            Dynamic
        }

        /// <summary>
        /// Specifies the type of pixel format used for color representation.
        /// </summary>
        public enum ColorSpaceType : byte
        {
            /// <summary>
            /// Red-Green-Blue color format, commonly used in displays and images.
            /// </summary>
            RGB = default,
            /// <summary>
            /// YUV color format, separates luminance (Y) from chrominance (U and V), used in video compression and broadcasting.
            /// R channel info stores Y (luminance), 
            /// G channel info stores U (chrominance blue), 
            /// B channel info stores V (chrominance red).
            /// </summary>
            YUV,
            /// <summary>
            /// Cyan-Magenta-Yellow-Black color format, primarily used in printing.
            /// R channel info stores C, 
            /// G channel info stores M, 
            /// B channel info stores Y,
            /// A channel info stores K.
            /// </summary>
            CMYK,
        }
    }
}
