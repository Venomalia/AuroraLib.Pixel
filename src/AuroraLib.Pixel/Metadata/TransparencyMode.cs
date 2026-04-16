namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Defines how transparency is handled for an image or material.
    /// </summary>
    public enum TransparencyMode : byte
    {
        /// <summary>
        /// No transparency is used, the image is fully opaque suitable for normal solid objects with no transparent areas.
        /// </summary>
        Opaque,

        /// <summary>
        /// Only fully transparent pixels are treated as transparent (binary cutout).
        /// </summary>
        Cutout,

        /// <summary>
        /// Allows Partial Transperancy using alpha blending.
        /// </summary>
        Transparent,

    }
}
