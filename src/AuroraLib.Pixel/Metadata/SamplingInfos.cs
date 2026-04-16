using AuroraLib.Pixel.PixelFormats;

namespace AuroraLib.Pixel.Metadata
{

    /// <summary>
    /// Describes how a texture is sampled during rendering.
    /// </summary>
    public sealed class SamplingInfos
    {
        /// <summary>
        /// The wrap mode for the S (U) coordinate.
        /// Specifies how textures outside the vertical range [0..1] are treated for text coordinates.
        /// </summary>
        public TextureWrapMode WrapS;

        /// <summary>
        /// The wrap mode for the T (V) coordinate.
        /// Specifies how textures outside the vertical range [0..1] are treated for text coordinates.
        /// </summary>
        public TextureWrapMode WrapT;

        /// <summary>
        /// The wrap mode for the R (W) coordinate.
        /// Specifies how textures outside the vertical range [0..1] are treated for text coordinates.
        /// Used mainly for 3D textures or texture arrays.
        /// </summary>
        public TextureWrapMode WrapR;

        /// <summary>
        /// The magnification filter used when a texture is displayed larger than its native resolution.
        /// </summary>
        public TextureFilter MagFilter;

        /// <summary>
        /// The minification filter used when a texture is displayed smaller than its native resolution.
        /// </summary>
        public TextureFilter MinFilter;

        /// <summary>
        /// Defines how transparency of the texture is handled during rendering.
        /// </summary>
        public TransparencyMode TransparencyMode;

        /// <summary>
        /// Level-of-detail bias applied during texture sampling.
        /// Positive values favor lower-resolution mip levels.
        /// </summary>
        public float LODBias;

        /// <summary>
        /// Minimum level-of-detail that may be sampled from the texture.
        /// Exclude textures below a certain LOD level from being used.
        /// </summary>
        public float MinLOD;

        /// <summary>
        /// Maximum level-of-detail that may be sampled from the texture.
        /// Exclude textures above a certain LOD level from being used.
        /// A value larger than the actual textures should lead to culling.
        /// </summary>
        public float MaxLOD;

        /// <summary>
        /// Maximum anisotropic filtering level used when sampling the texture.
        /// Higher values improve quality at oblique viewing angles.
        /// </summary>
        public float MaxAnisotropy;

        /// <summary>
        /// Border color used for out-of-range texture coordinates when using <see cref="TextureWrapMode.ClampToBorder"/> mode.
        /// </summary>
        public RGBA<float> BorderColor;
    }

}
