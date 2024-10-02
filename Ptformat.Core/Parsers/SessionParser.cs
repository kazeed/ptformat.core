using System;
using Microsoft.Extensions.Logging;
using PtInfo.Core.Model;
using PtInfo.Core.Utilities;
using System.Collections.Generic;
using PtInfo.Core.Extensions;
using System.Linq;

namespace PtInfo.Core.Parsers
{
    public class SessionParser(
        IListParser<Track> trackParser,
        ISingleParser<HeaderInfo> headerParser,
        ILogger<SessionParser> logger)
    {
        private readonly ISingleParser<HeaderInfo> headerParser = headerParser ?? throw new ArgumentNullException(nameof(headerParser));
        private readonly IListParser<Track> trackParser = trackParser ?? throw new ArgumentNullException(nameof(trackParser));
        private readonly ILogger<SessionParser> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Parses the given file data into a Session model.
        /// </summary>
        /// <param name="fileData">A PTS file, as a byte array.</param>
        /// <returns>A Session model, that contains all extracted info about the PTS file.</returns>
        /// <exception cref="ArgumentException">If the passed array is empty.</exception>
        public Session Parse(byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentException("File data cannot be null or empty.");
            }

            // Calculate endianness
            var isBigEndian = fileData[0x11] != 0x00;
            logger.LogInformation("Endianness determined: {Endianness}", isBigEndian ? "Big Endian" : "Little Endian");

            // Find and parse all blocks in the file
            var blocks = FindBlocks(fileData, isBigEndian);
            logger.LogInformation("Found {BlockCount} blocks in the file.", blocks.Count);

            // Parse header, reducing blocks to only those related to the header.
            var headerBlocks = new Queue<Block> (blocks.Where(b => b.ContentType == ContentType.InfoProductVersion || b.ContentType == ContentType.InfoSampleRate));
            var headerInfo = headerParser.Parse(headerBlocks, fileData, isBigEndian);
            logger.LogInformation("Parsed HeaderInfo: {SessionName}, {SampleRate}, {ProductVersion}",
                headerInfo.Name, headerInfo.SampleRate, headerInfo.ProductVersion);

            // Parse tracks, reducing blocks to only those related to the tracks.
            var otherBlocks = new Queue<Block>(blocks.Where(b => b.ContentType != ContentType.InfoProductVersion && b.ContentType != ContentType.InfoSampleRate));
            var tracks = trackParser.Parse(blocks, fileData, isBigEndian);

            // Construct and return the Session model
            var session = new Session
            {
                HeaderInfo = headerInfo,
                Tracks = tracks,
            };

            logger.LogInformation("Session parsing complete with {TrackCount} tracks.", session.Tracks.Count);

            return session;
        }

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
                Block block = ParseBlock(zMarkPos, rawFile, isBigEndian);
                blocks.Enqueue(block);
                logger.LogInformation("Found block at offset {offset}, Type: {blockType}, ContentType: {contentType}",
                    block.Offset, block.Type, block.ContentType);

                // Move the offset to the next position after the current block
                offset = zMarkPos + block.Size + 7; // 7 is the size of the header
            }

            logger.LogInformation("Total blocks found: {count}", blocks.Count);
            return blocks;
        }

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

                // Now parse any potential child blocks within the current block
                var childOffset = pos + 7; // Start after the header
                var maxOffset = pos + blockSize; // The max limit of the current block

                while (childOffset < maxOffset)
                {
                    var childBlock = ParseBlock(childOffset, rawFile, isBigEndian);
                    block.Children.Add(childBlock);

                    // Move to the next block, considering the size of the child block
                    childOffset += childBlock.Size + 7; // 7 is the header size
                }

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