using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Session
    {
        public HeaderInfo HeaderInfo { get; set; }
        public List<Track> Tracks { get; set; }
    }
}
