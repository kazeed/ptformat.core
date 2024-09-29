namespace Ptformat.Core.Model
{

    /// <summary>
    /// Represents metadata for a region within the PTS file, with start, offset, and length values.
    /// </summary>
    public record RegionMetadata(long Start, long Offset, long Length)
    {
        /// <summary>
        /// The end position of the region, calculated as Start + Length.
        /// </summary>
        public long End => Start + Length;

        /// <summary>
        /// Checks if this region overlaps with another region.
        /// </summary>
        /// <param name="other">The other RegionMetadata to check against.</param>
        /// <returns>True if there is an overlap; otherwise, false.</returns>
        public bool Overlaps(RegionMetadata other) =>
            Start < other.End && End > other.Start;
    }
}