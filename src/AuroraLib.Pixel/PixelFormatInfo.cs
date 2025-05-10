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
        /// Indicates whether the pixel format has dynamic channel.
        /// </summary>
        public bool HasDynamicChannel { get; }

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

        public PixelFormatInfo(byte bitsPerPixel, byte redDepth, byte redMaskShift, byte greenDepth, byte greenMaskShift, byte blueDepth, byte blueMaskShift, byte alphaDepth = 0, byte alphaMaskShift = 0, bool isDynamic = false)
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
            HasDynamicChannel = isDynamic;
        }

        public PixelFormatInfo(byte bitsPerPixel, ulong redMask, ulong greenMask, ulong blueMask, ulong alphaMask = 0)
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
            HasDynamicChannel = false;
        }
    }
}
