namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Determines how texture coordinates outside [0,1] are handled.
    /// </summary>
    public enum TextureWrapMode : byte
    {
        /// <summary>
        /// Coordinates outside [0,1] are clamped to the edge pixel.
        /// </summary>
        ClampToEdge,

        /// <summary>
        /// Coordinates wrap around, repeating the texture.
        /// </summary>
        Repeat,

        /// <summary>
        /// Coordinates mirror at every integer boundary.
        /// </summary>
        MirroredRepeat,

        /// <summary>
        /// Mirrors the texture once, then clamps to edge pixels.
        /// </summary>
        MirrorOnce,

        /// <summary>
        /// Coordinates outside [0,1] return a fixed border color defined by the sampler.
        /// </summary>
        ClampToBorder
    }
}
