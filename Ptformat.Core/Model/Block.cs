using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ptformat.Core.Model
{
    
    public class Block 
    {
        public Block()
        {
            this.Children = [];
        }

        public byte ZMark { get; set; }

        public int Type { get; set; }

        public int Size { get; set; }

        public ContentType ContentType { get; set; }

        public long Offset { get; set; }

        public Block? Parent { get; set; }

        public List<Block> Children { get; }
        public byte[] RawData { get; set; }
        /*
* struct block_t {
uint8_t zmark;			// 'Z'
uint16_t block_type;		// type of block
uint32_t block_size;		// size of block
uint16_t content_type;		// type of content
uint32_t offset;		// offset in file
std::vector<block_t> child;	// vector of child blocks
};*/
    }
}
