namespace AuroraLib.Pixel.Processing
{
    /// <summary>
    /// Specifies the hexadecimal string format used to represent a color.
    /// </summary>
    public enum HexColorFormat
    {
        /// <summary>Three-digit RGB format (#RGB).</summary>
        RGB = 3,
        /// <summary>Four-digit RGBA format (#RGBA).</summary>
        RGBA = 4,
        /// <summary>Six-digit RGB format (#RRGGBB).</summary>
        RRGGBB = 6,
        /// <summary>Eight-digit RGBA format (#RRGGBBAA).</summary>
        RRGGBBAA = 8,
    }
}
