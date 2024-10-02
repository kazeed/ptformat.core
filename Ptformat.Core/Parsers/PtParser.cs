using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Ptformat.Core.Extensions;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;

namespace Ptformat.Core.Parsers
{
    public class PtFileParser : IDisposable
    {
        private const int ZMARK = 0x5A;
        
        private readonly Queue<Block> blocks = [];
        private bool isBigEndian;
        private byte[] fileData;

        private readonly ILogger<PtFileParser> logger;
        private readonly IPtParser<AudioTrack> audioParser;
        private readonly IPtParser<Track> trackParser;

        private bool disposedValue;

        public PtFileParser(ILogger<PtFileParser> logger, IPtParser<AudioTrack> audioParser, IPtParser<Track> trackParser)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.audioParser = audioParser ?? throw new ArgumentNullException(nameof(audioParser));
            this.trackParser = trackParser ?? throw new ArgumentNullException(nameof(trackParser));
        }

        public Session Parse(byte[] fileData)
        {
            ArgumentNullException.ThrowIfNull(fileData);

            this.fileData = fileData;
            this.isBigEndian = fileData[0x11] != 0x00;
            FindBlocks();
            var audio = audioParser.Parse(blocks, fileData, isBigEndian);
            var tracks = trackParser.Parse(blocks, fileData, isBigEndian);

            var session = new Session
            {
                Blocks = [.. blocks],
                //Audio = audio,
                Tracks = tracks
            };

            return session;
        }
        
       
        /// <summary>
        /// Extracts and enqueues each valid block in the correct order.
        /// </summary>
        private void FindBlocks()
        {
            if (fileData == null || fileData.Length == 0) return;

            var offsets = GetBlockOffsets();

            foreach (var offset in offsets)
            {
                var block = ExtractBlock(offset);

                if (block != null && block.ContentType != ContentType.Invalid)
                {
                    blocks.Enqueue(block);
                    logger.LogInformation("Enqueued block at offset {offset} with type {type}, size {size}, content type {contentType}", offset, block.Type, block.Size, block.ContentType);
                }
            }
        }

        /// <summary>
        /// Parses the entire file to discover and interpret all block offsets.
        /// </summary>
        private List<int> GetBlockOffsets()
        {
            var markers = new List<int>();
            var dataSpan = new Span<byte>(fileData);
            var offset = 0;

            // Use IndexOf to find each ZMARK occurrence efficiently
            while (offset < fileData.Length)
            {
                var index = dataSpan[offset..].IndexOf((byte)ZMARK);
                if (index == -1) break; // No more ZMARK found

                // Calculate the absolute position
                offset += index;

                logger.LogInformation("Found ZMARK at offset {offset}", offset);
                markers.Add(offset);

                // Move to the next byte after the current ZMARK
                offset++;
            }

            logger.LogInformation("Total blocks parsed: {count}", markers.Count);
            return markers;
        }


        /// <summary>
        /// Extracts a block from the specified position in the file data.
        /// </summary>
        /// <param name="pos">The position in the file data to start extraction.</param>
        /// <returns>The extracted Block or null if invalid.</returns>
        private Block? ExtractBlock(int pos)
        {
            if (pos + 7 >= fileData.Length)
            {
                logger.LogWarning("Position {pos} is out of bounds, skipping block extraction.", pos);
                return null;
            }

            try
            {
                var blockType = EndianReader.ReadInt16(fileData, pos + 1, isBigEndian);
                if ((blockType & 0xFF00) == 0xFF00) return null; // Skip invalid block types
                var blockSize = EndianReader.ReadInt32(fileData, pos + 3, isBigEndian);
                var contentType = EndianReader.ReadInt16(fileData, pos + 7, isBigEndian);
                var rawData = ParserUtils.ReadBlockContent(fileData, pos + 7);

                var block = new Block
                {
                    ZMark = fileData[pos],
                    Offset = pos,
                    Type = blockType,
                    Size = blockSize,
                    ContentType = contentType.ToContentType(),
                    RawData = rawData,
                    Content = rawData.ParseFields(),
                    Children = []
                };

                logger.LogInformation("Extracted block at offset {pos}, Type: {blockType}, Size: {blockSize}, ContentType: {contentType}", pos, blockType, blockSize, contentType.ToContentType());
                return block;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to extract block at offset {pos}", pos);
                return null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose of managed resources here, if needed
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
