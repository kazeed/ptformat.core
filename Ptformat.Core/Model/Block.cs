using System.Collections.Generic;

namespace PtInfo.Core.Model
{

    public class Block 
    {
        public Block()
        {
            this.Children = [];
            this.Content = [];
            this.RawData = [];
        }

        public byte ZMark { get; set; }

        public int Type { get; set; }

        public int Size { get; set; }

        public ContentType ContentType { get; set; }

        public long Offset { get; set; }

        public Block? Parent { get; set; }

        public List<Block> Children { get; set; }
        public byte[] RawData { get; set; }
        public List<string> Content { get; init; }
    }
}
