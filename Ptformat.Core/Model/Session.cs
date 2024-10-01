using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptformat.Core.Model
{
    public class Session
    {
        public List<Block> Blocks { get; set; }
        public List<AudioRef> Audio { get; set; }
    }
}
