using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Track(string name)
    {
        // Name of the track (common for both audio and MIDI)
        public string Name { get; set; } = name;

        // List of regions associated with the track
        public List<Region> Regions { get; set; } = [];

        // List of channel numbers associated with the track (for both audio and MIDI)
        public List<int> Channels { get; set; } = [];

        // Method to add a region to the track
        public void AddRegion(Region region)
        {
            Regions.Add(region);
        }
    }
}