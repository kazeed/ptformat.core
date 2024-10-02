using System.Collections.Generic;

namespace PtInfo.Core.Model
{
    public class Track(string name)
    {
        // Name of the track (common for both audio and MIDI)
        public string Name { get; set; } = name;

        // List of regions associated with the track
        public List<Region> Regions { get; set; } = [];

        // Mono or stereo (1 or 2 channels)
        public List<int> Channels { get; set; } = [];

        // Method to add a region to the track
        public void AddRegion(AudioRegion region)
        {
            Regions.Add(region);
        }
    }
}