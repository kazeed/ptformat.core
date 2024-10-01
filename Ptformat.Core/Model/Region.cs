using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Region 
    {
        public string Name { get; set; } 

        RegionMetadata Metadata { get; set; } 

        public List<MidiEvent> Midi { get; } = [];
    }
}
