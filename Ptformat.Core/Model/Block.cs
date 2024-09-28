using System.Collections.Generic;

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

        public int ContentType { get; set; }

        public long Offset { get; set; }

        public List<Block> Children { get; }
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
