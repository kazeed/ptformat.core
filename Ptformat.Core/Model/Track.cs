using System;
using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Track(string name, int index, List<Region> regions)
    {
        public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));

        public int Index { get; set; } = index;

        public List<Region> Regions { get; set; } = regions ?? throw new ArgumentNullException(nameof(regions));
    }
}
