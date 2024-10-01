using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class Session
    {
        public List<Block> Blocks { get; set; }
        public List<AudioRef> Audio { get; set; }
    }
}
