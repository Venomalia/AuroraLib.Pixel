namespace AuroraLib.Pixel.Metadata
{

    /// <summary>
    /// Defines texture filtering modes for minification and magnification.
    /// </summary>
    public enum TextureFilter : byte
    {
        /// <summary>
        /// Uses the nearest texel without interpolation (fast, blocky look).
        /// </summary>
        Nearest,

        /// <summary>
        /// Uses linear interpolation between nearest texels (smooth, blurred look).
        /// </summary>
        Linear,

        /// <summary>
        /// Uses nearest texel from the nearest mip level.
        /// </summary>
        NearestMipmapNearest,

        /// <summary>
        /// Uses linear interpolation of texels from the nearest mip level.
        /// </summary>
        LinearMipmapNearest,

        /// <summary>
        /// Uses nearest texels from two adjacent mip levels and blends between them.
        /// </summary>
        NearestMipmapLinear,

        /// <summary>
        /// Uses linear interpolation of texels from two adjacent mip levels (trilinear).
        /// </summary>
        LinearMipmapLinear,
    }
}
