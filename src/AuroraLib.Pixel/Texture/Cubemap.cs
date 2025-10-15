using AuroraLib.Pixel.Image;
using System;

namespace AuroraLib.Pixel.Texture
{
    /// <summary>
    /// Represents a cubemap texture consisting of six faces: positive/negative X, Y, and Z.
    /// </summary>
    public sealed class Cubemap<TColor> : Texture<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the positive x texture.
        /// </summary>
        public FlatTexture<TColor> PositiveX { get; }

        /// <summary>
        /// Gets the negative x texture.
        /// </summary>
        public FlatTexture<TColor> NegativeX { get; }

        /// <summary>
        /// Gets the positive y texture.
        /// </summary>
        public FlatTexture<TColor> PositiveY { get; }

        /// <summary>
        /// Gets the negative y texture.
        /// </summary>
        public FlatTexture<TColor> NegativeY { get; }

        /// <summary>
        /// Gets the positive z texture.
        /// </summary>
        public FlatTexture<TColor> PositiveZ { get; }

        /// <summary>
        /// Gets the negative z texture.
        /// </summary>
        public FlatTexture<TColor> NegativeZ { get; }

        /// <inheritdoc/>
        public override int LevelCount => 6;

        /// <summary>
        /// Initializes a new cubemap where all six faces are clones of the provided base surface.
        /// </summary>
        /// <param name="baseSurface">The base texture to clone for all six faces of the cubemap.</param>
        public Cubemap(FlatTexture<TColor> baseSurface)
        {
            PositiveX = baseSurface;
            NegativeX = (FlatTexture<TColor>)baseSurface.Clone();
            PositiveY = (FlatTexture<TColor>)baseSurface.Clone();
            NegativeY = (FlatTexture<TColor>)baseSurface.Clone();
            PositiveZ = (FlatTexture<TColor>)baseSurface.Clone();
            NegativeZ = (FlatTexture<TColor>)baseSurface.Clone();
        }

        /// <summary>
        /// Initializes a new cubemap using the explicitly provided textures for each face.
        /// </summary>
        /// <param name="positiveX">Texture for the positive X face.</param>
        /// <param name="negativeX">Texture for the negative X face.</param>
        /// <param name="positiveY">Texture for the positive Y face.</param>
        /// <param name="negativeY">Texture for the negative Y face.</param>
        /// <param name="positiveZ">Texture for the positive Z face.</param>
        /// <param name="negativeZ">Texture for the negative Z face.</param>
        public Cubemap(FlatTexture<TColor> positiveX, FlatTexture<TColor> negativeX, FlatTexture<TColor> positiveY, FlatTexture<TColor> negativeY, FlatTexture<TColor> positiveZ, FlatTexture<TColor> negativeZ)
        {
            PositiveX = positiveX;
            NegativeX = negativeX;
            PositiveY = positiveY;
            NegativeY = negativeY;
            PositiveZ = positiveZ;
            NegativeZ = negativeZ;

            int baseWidth = positiveX.Width, baseHeight = positiveX.Height;
            foreach (var face in this)
            {
                if (face.Width != baseWidth || face.Height != baseHeight)
                    throw new ArgumentException("All cubemap faces must have the same dimensions.");
            }
        }

        /// <inheritdoc/>
        public override IImage<TColor> GetLevel(int index) => index switch
        {
            0 => PositiveX,
            1 => NegativeX,
            2 => PositiveY,
            3 => NegativeY,
            4 => PositiveZ,
            5 => NegativeZ,
            _ => throw new IndexOutOfRangeException($"Cubemap face index must be between 0 and 5, but was {index}.")
        };

        /// <inheritdoc/>
        public override IImage<TColor> Clone()
            => new Cubemap<TColor>((FlatTexture<TColor>)PositiveX.Clone(), (FlatTexture<TColor>)NegativeX.Clone(), (FlatTexture<TColor>)PositiveY.Clone(), (FlatTexture<TColor>)NegativeY.Clone(), (FlatTexture<TColor>)PositiveZ.Clone(), (FlatTexture<TColor>)NegativeZ.Clone());

        /// <inheritdoc/>
        public override IImage<TColor> Create(int width, int height)
            => new Cubemap<TColor>((FlatTexture<TColor>)NegativeX.Create(width, height));
    }
}
