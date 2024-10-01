using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Ptformat.Core.Extensions;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;

namespace Ptformat.Core.Parsers
{
    public class PtFileParser : IDisposable
    {
        private const int ZMARK = 0x5A;

        private readonly byte[] fileData;
        private readonly ILogger<PtFileParser> logger;
        private readonly bool isBigEndian;
        private bool disposedValue; // For implementing IDisposable
        private readonly Stack<Block> blocks = [];
       
        public PtFileParser(string filePath, ILogger<PtFileParser> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Load the file data into memory
            fileData = File.ReadAllBytes(filePath);
            isBigEndian = fileData[0x11] != 0x00;
        }

        public Session Parse()
        {

            var offsets = GetBlockOffsets();
            var parsedBlocks = offsets.Select(o => ParseBlock(o)).Where(b => b != null);
            foreach (var block in parsedBlocks)
            {
                blocks.Push(block);
            }
            var audio = ParseAudio();

            var session = new Session
            {
                Blocks = [.. blocks],
                Audio = audio
            };

            return session;
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
        /// Parses a block at the specified offset and recursively parses its child blocks.
        /// </summary>
        private Block ParseBlock(int pos, Block? parent = null)
        {
            if (pos + 7 >= fileData.Length)
            {
                logger.LogWarning("Position {pos} is out of bounds, skipping block parsing.", pos);
                throw new IndexOutOfRangeException("$Position {pos} is out of bounds, skipping block parsing.");
            }

            try
            {
                var blockType = EndianReader.ReadInt16(fileData, pos + 1, isBigEndian);
                if ((blockType & 0xFF00) == 0xFF00) return new Block { ContentType = ContentType.Invalid }; // Skip invalid block types
                var blockSize = EndianReader.ReadInt32(fileData, pos + 3, isBigEndian);
                var contentType = EndianReader.ReadInt16(fileData, pos + 7, isBigEndian);
                var rawData = ReadBlockContent(pos + 7);

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

                logger.LogInformation("Parsed block at offset {pos}, Type: {blockType}, Size: {blockSize}, ContentType: {contentType}",
                    pos, blockType, blockSize, contentType.ToContentType());

                // Calculate the maximum offset within this block
                var maxOffset = parent != null ? parent.Offset + parent.Size : fileData.Length;

                // Recursively parse potential child blocks within this block
                var childJump = 0;
                for (var i = 1; (i < blockSize) && (pos + i + childJump < maxOffset); i += (childJump != 0 ? childJump : 1))
                {
                    var childPos = pos + i;
                    childJump = 0;

                    var childBlock = ParseBlock(childPos, block);
                    if (childBlock != null)
                    {
                        block.Children.Add(childBlock);
                        childJump = childBlock.Size + 7; // Include the header size (7 bytes)

                        logger.LogInformation("Child block parsed at offset {childPos}, Parent offset {parentPos}, Child Type: {childType}, Size: {childSize}, ContentType: {childContentType}",
                            childPos, pos, childBlock.Type, childBlock.Size, childBlock.ContentType);
                    }
                    else
                    {
                        logger.LogWarning("Failed to parse child block at offset {childPos} within parent block at {parentPos}", childPos, pos);
                    }
                }

                logger.LogInformation("Completed parsing block at offset {pos} with {childCount} child blocks.", pos, block.Children.Count);
                return block;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse block header at offset {pos}", pos);
                throw;
            }
        }

        /// <summary>
        /// Parses all blocks in the file data to extract audio file information.
        /// </summary>
        private List<AudioRef> ParseAudio() => blocks
                .Where(b => b.ContentType == ContentType.WavListFull)
                .SelectMany(ConvertoToAudioRefs)
                .Select(aRef => aRef.AddLength([.. blocks], logger))
                .ToList();

        /// <summary>
        /// Extracts WAV files from a block with the content type WavListFull (0x1004).
        /// </summary>
        private List<AudioRef> ConvertoToAudioRefs(Block block)
        {
            var wavFiles = new List<AudioRef>();
            var nwavs = EndianReader.ReadInt32(fileData, (int)block.Offset + 2, isBigEndian);

            foreach (var child in block.Children.Where(c => c.ContentType == ContentType.WavNames))
            {
                var pos = (int)child.Offset + 11;

                for (int i = 0, n = 0; (pos < child.Offset + child.Size) && (n < nwavs); i++)
                {
                    var wavName = ParseString(pos); 
                    pos += wavName.Length + 4;

                    var wavType = Encoding.ASCII.GetString(fileData, pos, 4);
                    pos += 9;

                    if (ParserUtils.IsInvalidWavNameOrType(wavName, wavType))
                        continue;

                    wavFiles.Add(new AudioRef(n, wavName));
                    n++;
                }
            }

            return wavFiles;
        }

        /// <summary>
        /// Reads the content of a block from the given start offset to the next ZMARK (0x5A) or end of the stream.
        /// </summary>
        private byte[] ReadBlockContent(int startOffset)
        {
            int endOffset = startOffset;
            while (endOffset < fileData.Length && fileData[endOffset] != ZMARK)
            {
                endOffset++;
            }

            var blockContent = new byte[endOffset - startOffset];
            Array.Copy(fileData, startOffset, blockContent, 0, blockContent.Length);
            logger.LogInformation("Block content read from offset {startOffset} to {endOffset}.", startOffset, endOffset);

            return blockContent;
        }

        /// <summary>
        /// Parses a string from the specified position in the file data.
        /// The method reads the length of the string first and then extracts the string content.
        /// </summary>
        /// <param name="position">The starting position in the data array to parse the string.</param>
        /// <returns>The parsed string.</returns>
        private string ParseString(int position)
        {
            // Ensure we're within bounds
            if (position + 4 >= fileData.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of fileData bounds.");

            // Read the length of the string (4 bytes) using EndianReader
            var length = EndianReader.ReadInt32(fileData, position, isBigEndian);

            // Move the position forward by 4 bytes to read the actual string content
            position += 4;

            // Ensure the string length is within the bounds
            if (position + length > fileData.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "String length exceeds fileData bounds.");

            // Extract the string content
            var content = Encoding.ASCII.GetString(fileData, position, length).TrimEnd('\0');

            return content;
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
