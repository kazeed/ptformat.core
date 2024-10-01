using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Readers;
using Ptformat.Core.Utilities;

namespace Ptformat.Core.Parsers
{
    public class PtFileParser : IDisposable
    {
        private const int ZMARK = 0x5A;
        private const int FMARK = 0x3F;

        private readonly byte[] fileData;
        private readonly ILogger<PtFileParser> logger;
        private readonly bool isBigEndian;
        private bool isLoaded;

        // Store parsed blocks
        private bool disposedValue; // For implementing IDisposable
        private List<Block> blocks;
       

        public PtFileParser(string filePath, ILogger<PtFileParser> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Load the file data into memory
            fileData = File.ReadAllBytes(filePath);
            isBigEndian = fileData[0x11] != 0x00;
            isLoaded = true;
        }


        public List<Block> Parse()
        {
            if (!isLoaded) throw new InvalidOperationException("File data must be loaded before parsing.");

            blocks = GetBlockOffsets()
                .Select(o => ParseBlock(o))
                .Where(b => b != null)
                .ToList();

            return blocks;
        }

        /// <summary>
        /// Parses the audio blocks to extract WAV files with relevant metadata.
        /// </summary>
        /// <returns>A list of WavFile objects representing the audio files in the session.</returns>
        public List<WavFile> ParseAudio()
        {
            // Step 1: Parse WAV list block
            var wavFiles = ParseWavListFull();

            // Step 2: Add length information to the WAV files
            AddWavLengthInformation(wavFiles);

            return wavFiles;
        }

        /// <summary>
        /// Parses the entire file to discover and interpret all block offsets.
        /// </summary>
        private List<int> GetBlockOffsets()
        {
            if (!isLoaded) throw new InvalidOperationException("File data must be loaded before parsing.");

            var markers = new List<int>();
            var dataSpan = new Span<byte>(fileData);
            var offset = 0;

            // Use IndexOf to find each ZMARK occurrence efficiently
            while (offset < fileData.Length)
            {
                var index = dataSpan.Slice(offset).IndexOf((byte)ZMARK);
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
        private Block? ParseBlock(int pos, Block? parent = null)
        {
            if (pos + 7 >= fileData.Length)
            {
                logger.LogWarning("Position {pos} is out of bounds, skipping block parsing.", pos);
                throw new IndexOutOfRangeException("$Position {pos} is out of bounds, skipping block parsing.");
            }

            try
            {
                var blockType = EndianReader.ReadInt16(fileData, pos + 1, isBigEndian);
                if (blockType == 0) return null; // Skip invalid block types
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
                    Content = ParseFields(rawData),
                    Children = new List<Block>()
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
        /// Parses fields from the raw block data using 0x3F as the field separator.
        /// </summary>
        /// <param name="rawData">The raw data of the block.</param>
        /// <returns>A list of fields as strings.</returns>
        private List<string> ParseFields(byte[] rawData)
        {
            var fields = new List<string>();
            var start = 0;

            for (var i = 0; i < rawData.Length; i++)
            {
                // Check for field separator (0x3F) or the end of the block
                if (rawData[i] == FMARK || i == rawData.Length - 1)
                {
                    // Adjust length for the last field edge case
                    var length = (i == rawData.Length - 1) ? (i - start + 1) : (i - start);
                    if (length > 0)
                    {
                        // Extract the field content once using Encoding.ASCII
                        var fieldContent = Encoding.ASCII.GetString(rawData, start, length).TrimEnd('\0');
                        fields.Add(fieldContent);
                    }

                    // Update start index
                    start = i + 1;
                }
            }

            return fields;
        }

        /// <summary>
        /// Reads the content of a block from the given start offset to the next ZMARK (0x5A) or end of the stream.
        /// </summary>
        public byte[] ReadBlockContent(int startOffset)
        {
            if (!isLoaded) throw new InvalidOperationException("File data must be loaded before reading.");

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
        /// Parses the WavListFull block to extract WAV file names and types.
        /// </summary>
        /// <returns>A list of WavFile objects with filename and index information.</returns>
        private List<WavFile> ParseWavListFull()
        {
            var wavFiles = new List<WavFile>();

            // Find the block with ContentType corresponding to WavListFull (0x1004)
            var wavListBlock = blocks.FirstOrDefault(b => b.ContentType == ContentType.WavListFull);

            if (wavListBlock == null)
            {
                logger.LogWarning("No WAV list block found.");
                return wavFiles;
            }

            var nwavs = EndianReader.ReadInt32(fileData, (int)wavListBlock.Offset + 2, isBigEndian);

            // Find the child block with ContentType corresponding to WavNames (0x103a)
            var wavNamesBlock = wavListBlock.Children.FirstOrDefault(c => c.ContentType == ContentType.WavNames);

            if (wavNamesBlock == null)
            {
                logger.LogWarning("No WAV names block found within the WAV list.");
                return wavFiles;
            }

            var pos = (int)wavNamesBlock.Offset + 11; // Starting position within the block's data
            var index = 0;

            // Parse the WAV names
            while (pos < wavNamesBlock.Offset + wavNamesBlock.Size && index < nwavs)
            {
                var wavName = ParseString(pos);
                pos += wavName.Length + 4;

                // Extract WAV type (4 bytes)
                var wavType = Encoding.ASCII.GetString(fileData, pos, 4);
                pos += 9;

                // Skip entries that are not valid audio files
                if (ParserUtils.IsInvalidWavNameOrType(wavName, wavType)) continue;

                wavFiles.Add(new WavFile(index++, wavName));
            }

            return wavFiles;
        }

        /// <summary>
        /// Adds length information to each WavFile object.
        /// </summary>
        /// <param name="wavFiles">List of WavFile objects to update with length information.</param>
        private void AddWavLengthInformation(List<WavFile> wavFiles)
        {
            foreach (var block in blocks.Where(b => b.ContentType == ContentType.WavListFull))
            {
                var wavMetadataBlock = block.Children.FirstOrDefault(c => c.ContentType == ContentType.WavMetadata);

                if (wavMetadataBlock == null) continue;

                foreach (var lengthBlock in wavMetadataBlock.Children.Where(d => d.ContentType == ContentType.WavSampleRateSize))
                {
                    var length = EndianReader.ReadInt64(fileData, (int)lengthBlock.Offset + 8, isBigEndian);

                    var wavFile = wavFiles.FirstOrDefault(w => w.Index == wavFiles.IndexOf(w));
                    if (wavFile != null)
                    {
                        wavFile.Length = length;
                        logger.LogInformation("Updated WAV file {wavFile} with length: {length}", wavFile.Filename, length);
                    }
                }
            }
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
