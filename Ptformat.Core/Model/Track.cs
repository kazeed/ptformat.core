using System;
using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Track
    {
        public string Name { get; set; }

        public List<Region> Regions { get; set; }

        public List<int> Channels { get; internal set; }
    }
}
