using System.Collections.Generic;

namespace Ptformat.Core.Library
{
    // Represents a parsed audio track
    public class AudioTrack
    {
        public string Name { get; set; }

        public List<AudioRegion> Regions { get; } = [];
    }
}