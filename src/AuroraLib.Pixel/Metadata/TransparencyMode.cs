namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Defines how transparency is handled for an image or material.
    /// </summary>
    public enum TransparencyMode : byte
    {
        /// <summary>
        /// No transparency is used, all pixels are fully opaque.
        /// </summary>
        Opaque,

        /// <summary>
        /// Only fully opaque and fully transparent pixels are used (binary cutout).
        /// </summary>
        Cutout,

        /// <summary>
        /// Partial transparency using straight alpha, where RGB channels are stored independently of alpha.
        /// </summary>
        Straight,

        /// <summary>
        /// Partial transparency using premultiplied alpha, where RGB channels are multiplied by alpha.
        /// </summary>
        Premultiplied
    }
}
