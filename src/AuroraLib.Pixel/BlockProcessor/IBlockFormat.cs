namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Defines basic block metadata used in block-based image or texture compression.
    /// </summary>
    public interface IBlockFormat
    {
        /// <summary>
        /// Gets the width in pixel of the block.
        /// </summary>
        int BlockWidth { get; }

        /// <summary>
        /// Gets the height in pixel of the block.
        /// </summary>
        int BlockHeight { get; }

        /// <summary>
        /// Gets the number of byte per block.
        /// </summary>
        int BytesPerBlock { get; }
    }
}
