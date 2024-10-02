using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System.Collections.Generic;
using System;
using Ptformat.Core.Extensions;

namespace Ptformat.Core.Parsers
{
    public class SessionParser(
        IListParser<AudioTrack> audioParser,
        IListParser<MidiTrack> midiParser,
        IListParser<Region> audioRegionParser,
        IListParser<Region> compoundRegionParser,
        ISingleParser<HeaderInfo> headerParser,
        ILogger<SessionParser> logger)
    {
        private readonly IListParser<AudioTrack> audioParser = audioParser ?? throw new ArgumentNullException(nameof(audioParser));
        private readonly IListParser<MidiTrack> midiParser = midiParser ?? throw new ArgumentNullException(nameof(midiParser));
        private readonly IListParser<Region> audioRegionParser = audioRegionParser ?? throw new ArgumentNullException(nameof(audioRegionParser));
        private readonly IListParser<Region> compoundRegionParser = compoundRegionParser ?? throw new ArgumentNullException(nameof(compoundRegionParser));
        private readonly ISingleParser<HeaderInfo> headerParser = headerParser ?? throw new ArgumentNullException(nameof(headerParser));
        private readonly ILogger<SessionParser> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Session Parse(byte[] rawFile)
        {
            // 1. Determine endianness
            bool isBigEndian = rawFile[0x11] != 0x00;

            // 2. Find all blocks
            var blockQueue = FindBlocks(rawFile, isBigEndian);

            // 3. Parse the header for session metadata
            var headerInfo = headerParser.Parse(blockQueue, rawFile, isBigEndian);

            // 4. Parse audio tracks
            var audioTracks = audioParser.Parse(blockQueue, rawFile, isBigEndian);

            // 5. Parse MIDI tracks
            var midiTracks = midiParser.Parse(blockQueue, rawFile, isBigEndian);

            // 6. Parse audio regions
            var audioRegions = audioRegionParser.Parse(blockQueue, rawFile, isBigEndian);

            // 7. Parse compound regions
            var compoundRegions = compoundRegionParser.Parse(blockQueue, rawFile, isBigEndian);

            // 8. Build the final session object
            var session = new Session
            {
                HeaderInfo = headerInfo,
                AudioTracks = audioTracks,
                MidiTracks = midiTracks,
                AudioRegions = audioRegions,
                CompoundRegions = compoundRegions
            };

            logger.LogInformation("Session parsing complete.");

            return session;
        }

        /// <summary>
        /// Extracts all blocks from the raw file data and returns them in a queue for further parsing.
        /// </summary>
        private Queue<Block> FindBlocks(byte[] rawFile, bool isBigEndian)
        {
            const int ZMARK = 0x5A;
            var blocks = new Queue<Block>();
            var fileLength = rawFile.Length;
            var offset = 0;

            while (offset < fileLength)
            {
                // Find the next ZMARK, which indicates the start of a block
                var zMarkPos = Array.IndexOf(rawFile, (byte)ZMARK, offset);
                if (zMarkPos == -1) break; // No more ZMARKs, exit the loop

                // Parse the block at the found ZMARK position
                var block = ParseBlock(zMarkPos, rawFile, isBigEndian);
                if (block != null)
                {
                    blocks.Enqueue(block);
                    logger.LogInformation("Found block at offset {offset}, Type: {blockType}, ContentType: {contentType}",
                        block.Offset, block.Type, block.ContentType);
                }

                // Move the offset to the next position after the current block
                offset = zMarkPos + block.Size + 7; // 7 is the size of the header
            }

            logger.LogInformation("Total blocks found: {count}", blocks.Count);
            return blocks;
        }

        /// <summary>
        /// Parses a block at the specified position in the raw file data.
        /// </summary>
        private Block ParseBlock(int pos, byte[] rawFile, bool isBigEndian)
        {
            if (pos + 7 >= rawFile.Length)
            {
                logger.LogWarning("Position {pos} is out of bounds, skipping block parsing.", pos);
                throw new PtsParsingException("Position is out of bounds.");
            }

            try
            {
                var blockType = EndianReader.ReadInt16(rawFile, pos + 1, isBigEndian);
                if ((blockType & 0xFF00) == 0xFF00) throw new PtsParsingException("Invalid block");

                var blockSize = EndianReader.ReadInt32(rawFile, pos + 3, isBigEndian);
                var contentType = EndianReader.ReadInt16(rawFile, pos + 7, isBigEndian);
                var rawData = ParserUtils.ReadBlockContent(rawFile, pos + 7);

                var block = new Block
                {
                    ZMark = rawFile[pos],
                    Offset = pos,
                    Type = blockType,
                    Size = blockSize,
                    ContentType = contentType.ToContentType(),
                    RawData = rawData,
                    Content = rawData.ParseFields(),
                    Children = []
                };

                logger.LogInformation("Parsed block at offset {pos}, Type: {blockType}, Size: {blockSize}, ContentType: {contentType}",
                    pos, blockType, blockSize, contentType.ToContentType());

                return block;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse block at offset {pos}", pos);
                throw;
            }
        }
    }
}