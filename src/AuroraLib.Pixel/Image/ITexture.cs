using System.Collections.Generic;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a multi-level texture where each level is an image variant, such as mipmaps, animation frames or layers.
    /// </summary>
    /// <typeparam name="TColor">The pixel color type.</typeparam>
    public interface ITexture<TColor> : IImage<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the list of levels in the texture.
        /// </summary>
        List<IImage<TColor>> Levels { get; }
    }

}
