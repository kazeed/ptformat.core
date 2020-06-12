using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ptformat.Core.Model
{
    public class Region : IEquatable<Region>
    {
        public Region()
        {
            this.Midi = new List<MidiEvent>();
        }

        public string Name { get; set; }

        public int Index { get; set; }

        public long StartPosition { get; set; }

        public long SampleOffset { get; set; }

        public long Length { get; set; }

        public Wave Wave { get; set; }

        public List<MidiEvent> Midi { get; }

        public bool Equals([AllowNull] Region other)
        {
            if (other is null) return false;

            if (this.Index == other.Index) return true;

            if (this.Name.ToUpperInvariant().Equals(other.Name.ToUpperInvariant())) return true;

            return false;
        }

        public override bool Equals(object obj) => Equals(obj as Region);
    }
}
