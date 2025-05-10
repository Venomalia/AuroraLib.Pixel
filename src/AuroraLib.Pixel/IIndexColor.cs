namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents an index-based color, often used in palette-based images.
    /// </summary>
    public interface IIndexColor : IColor
    {
        /// <summary>
        /// The index value that corresponds to the color in the palette.
        /// </summary>
        int I { get; set; }
    }
}
