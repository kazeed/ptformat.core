namespace Ptformat.Core.Library
{
    // Represents a parsed audio region
    public class AudioRegion
    {
        public long StartTime { get; set; }

        public long EndTime { get; set; }

        public string FileName { get; set; }

        public string Name { get; internal set; }
    }
}