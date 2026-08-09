namespace AuroraLib.Pixel.Processing.Helper
{
    public readonly struct Kernel
    {
        /// <summary>
        /// First source pixel.
        /// </summary>
        public readonly int Start;

        /// <summary>
        /// Number of source pixels.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Offset into the weight buffer.
        /// </summary>
        public readonly int WeightOffset;

        public Kernel(int start, int length, int weightOffset)
        {
            Start = start;
            Length = length;
            WeightOffset = weightOffset;
        }
    }
}
