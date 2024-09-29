using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ptformat.Core.Model
{
    public class Region 
    {
        public Region()
        {
            this.Midi = [];
        }

        public string Name { get; set; }

        RegionMetadata Metadata { get; set; }

        public WavFile Wave { get; set; }

        public List<MidiEvent> Midi { get; }
    }
}
