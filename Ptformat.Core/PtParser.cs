using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ptformat.Core.Readers;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Formats.Tar;

namespace Ptformat.Core.Parsers
{
    public class PtFileParser(ILoggerFactory loggerFactory) : IDisposable
    {
        private const byte ZMARK = 0x5A; // Assuming ZMARK corresponds to byte 0x5A
        private const int HeaderSize = 20;
        private const int EndianIndicatorOffset = 0x11;
        private const int BitCodeSearchLimit = 0x100;
        private bool isBigEndian;
        private int sessionRate;
        // TODO: Load this in a load method, make other methods fail if not loaded
        private byte[] fileData; // This stores the unxored data
        private int version; // Only up to version 10 is supported

        private readonly ILoggerFactory loggerFactory = loggerFactory;
        private readonly ILogger logger = loggerFactory.CreateLogger<PtFileParser>();
        private FileStream fileStream;
        private XorDecoderReader xorReader;



        private readonly List<Block> parsedBlocks = []; // Store parsed blocks
        private static readonly string[] ExcludeAudioBlocks = new[] { ".grp", "Audio Files", "Fade Files" };

        /// <summary>
        /// Parses the version block of the PTS file at the given path.
        /// </summary>
        public async Task<Block> ParseVersion(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path), "Path cannot be null or empty.");

            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                xorReader = new XorDecoderReader(fileStream, loggerFactory.CreateLogger<XorDecoderReader>());

                fileData = (await xorReader.ReadToEndAsync()).Select(s => s.AsByte()).ToArray();

                // Check for valid file header using the FindAt extension method
                if (fileData[0] != 0x03 && !(fileData.FoundAt([byte.MaxValue]) > 0))
                {
                    logger.LogWarning("The PTS file does not have the expected format or BITCODE header.");
                    throw new PtsParsingException($"Invalid header detected at offset {BitCodeSearchLimit}", BitCodeSearchLimit);
                }

                isBigEndian = fileData[EndianIndicatorOffset] != 0;
                logger.LogInformation("Endian type determined: {isBigEndian}", isBigEndian);

                var block = TryParseBlock(fileData, 0x1f, null, 0);

                return block ?? throw new PtsParsingException("Unable to parse the version block.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while parsing the PTS file.");
                throw;
            }
        }

        /// <summary>
        /// Parses the header to find and extract the session rate.
        /// </summary>
        /// <returns>True if the session rate block is found and parsed; otherwise, false.</returns>
        public Block? ParseHeader()
        {
            // Assuming ContentType.SessionRate corresponds to 0x1028
            var sessionRateBlock = parsedBlocks.FirstOrDefault(b => b.ContentType == ContentType.InfoSampleRate); 
            if (sessionRateBlock != null)
            {
                // Read the session rate from the block
                sessionRate = EndianReader.ReadInt32(sessionRateBlock.RawData, (int)sessionRateBlock.Offset + 4, isBigEndian);
                logger.LogInformation("Parsed session rate: {_sessionRate}", sessionRate);
                return sessionRateBlock;
            }

            logger.LogWarning("Session rate block not found.");
            return null;
        }

        /// <summary>
        /// Parses all blocks in the PTS file and adds them to the parsedBlocks list.
        /// </summary>
        public async Task ParseAllBlocksAsync(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(path));

            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                xorReader = new XorDecoderReader(fileStream, loggerFactory.CreateLogger<XorDecoderReader>());

                fileData = (await xorReader.ReadToEndAsync()).Select(s => s.AsByte()).ToArray();
                isBigEndian = fileData[EndianIndicatorOffset] != 0;
                logger.LogInformation("Endian type determined: {isBigEndian}", isBigEndian);

                long i = HeaderSize; // Start parsing after the first 20 bytes
                while (i < fileData.Length)
                {
                    var block = TryParseBlock(fileData, (int)i, null, 0);
                    if (block != null)
                    {
                        parsedBlocks.Add(block);
                    }

                    // Move to the next block (including the 7-byte header) or one byte forward if no block was found
                    i = block == null ? i++ : i += block.Size + 7;  
                }

                logger.LogInformation("Parsed a total of {count} blocks from the file.", parsedBlocks.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while parsing blocks.");
                throw;
            }
        }

        /// <summary>
        /// Attempts to parse a block from the specified offset.
        /// </summary>
        private Block? TryParseBlock(byte[] data, int pos, Block? parent, int level)
        {
            try
            {
                var max = (long)data.Length;
                if (parent != null)
                    max = parent.Size + parent.Offset;

                if (data[pos] != ZMARK)
                {
                    return null;
                }

                var block = new Block
                {
                    ZMark = ZMARK,
                    Type = EndianReader.ReadInt16(data, pos + 1, isBigEndian),
                    Size= EndianReader.ReadInt32(data, pos + 3, isBigEndian),
                    ContentType = EndianReader.ReadInt16(data, pos + 7, isBigEndian).ToContentType(),
                    RawData = data,
                    Offset = pos + 7 // Include the header size (7 bytes)
                };

                if (block.Size + block.Offset > max || (block.Size & 0xff00) != 0)
                {
                    return null;
                }

                logger.LogInformation("Parsed Block at {pos} with Type: {BlockType}, Size: {BlockSize}, ContentType: {ContentType}", pos, block.Type, block.Size, block.ContentType);

                int childJump = 0;
                for (int i = 1; (i < block.Size) && (pos + i + childJump < max); i += (childJump != 0 ? childJump : 1))
                {
                    int currentPos = pos + i;
                    childJump = 0;

                    var child = TryParseBlock(data, currentPos, block, level + 1);
                    if (child != null)
                    {
                        var childBlock = child ?? new Block(); ;
                        block.Children.Add(childBlock);
                        childJump = childBlock.Size + 7; // Include the header size (7 bytes)
                    }
                }

                return block;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse block at offset {offset}", pos);
                return null;
            }
        }

        /// <summary>
        /// Parses a string from the specified position in the data array.
        /// </summary>
        /// <param name="pos">The position in the data array to start parsing.</param>
        /// <returns>The parsed string.</returns>
        public string ParseString(int pos, byte[] data)
        {
            // Read the 4-byte length value considering endianness
            int length = EndianReader.ReadInt32(data, pos, isBigEndian);

            // Move the position forward by 4 bytes to start reading the actual string content
            pos += 4;

            // Extract and return the string content from the data array
            return Encoding.ASCII.GetString(data, pos, length);
        }

        /// <summary>
        /// Parses audio blocks and returns a list of parsed WavFiles.
        /// </summary>
        /// <returns>A list of parsed WavFiles.</returns>
        public List<WavFile> ParseAudio()
        {
            // Step 1: Parse WAV names using LINQ
            var wavBlocks = parsedBlocks
                .Where(b => b.ContentType == ContentType.WavListFull)
                .SelectMany(b => b.Children.Where(c => c.ContentType == ContentType.WavNames));

            // Get the list of valid WAV file paths
            var wavFiles = ParseWavNames(wavBlocks)
                .Select(w =>  AddWavLengthInformation(w))
                .ToList();
            
            if (wavFiles.Count == 0)
            {
                logger.LogWarning("No valid WAV files were found.");
            }

            return wavFiles;
        }

        /// <summary>
        /// Parses the WAV names from the given blocks and returns a list of WavFile objects with filenames.
        /// </summary>
        /// <param name="wavBlocks">Blocks to parse for WAV names.</param>
        /// <returns>A list of WavFile objects representing the WAV files.</returns>
        private List<WavFile> ParseWavNames(IEnumerable<Block> wavBlocks)
        {
            var wavFiles = new List<WavFile>();

            foreach (var block in wavBlocks)
            {
                if (block.Parent == null) continue;

                int pos = (int)block.Offset + 11;
                int nwavs = EndianReader.ReadInt32(fileData, (int)block.Parent.Offset + 2, isBigEndian);

                for (int i = 0, n = 0; (pos < block.Offset + block.Size) && (n < nwavs); i++)
                {
                    string wavName = ParseString(pos, fileData);
                    pos += wavName.Length + 4;
                    string wavType = Encoding.ASCII.GetString(fileData, pos, 4);
                    pos += 9;

                    if (IsInvalidWavNameOrType(wavName, wavType))
                        continue;

                    wavFiles.Add(new WavFile(n) { Filename = wavName });
                    n++;
                }
            }

            return wavFiles;
        }
        
        /// <summary>
        /// Adds length information to a single WavFile.
        /// </summary>
        /// <param name="wavFile">The WavFile object to update with length information.</param>
        /// <returns>The updated WavFile object with length information, or null if not found.</returns>
        private WavFile AddWavLengthInformation(WavFile wavFile)
        {
            var lengthBlock = parsedBlocks
                .Where(b => b.ContentType == ContentType.WavListFull)
                .SelectMany(b => b.Children
                    .Where(c => c.ContentType == ContentType.WavMetadata)
                    .SelectMany(c => c.Children.Where(d => d.ContentType == ContentType.WavSampleRateSize)))
                .FirstOrDefault();

            if (lengthBlock == null)
            {
                logger.LogWarning("No length block found for WavFile: {Filename}", wavFile.Filename);
                return wavFile; // Return the WavFile without length information
            }

            try
            {
                long length = EndianReader.ReadInt64(fileData, (int)lengthBlock.Offset + 8, isBigEndian);
                wavFile.Length = length;
                logger.LogInformation("WavFile {Filename} updated with length: {Length}", wavFile.Filename, length);
                return wavFile;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while reading length for WavFile: {Filename}", wavFile.Filename);
                return wavFile; // Return the WavFile without length information
            }
        }


        /// <summary>
        /// Determines if a WAV name or type is invalid based on the file version and expected formats.
        /// </summary>
        private bool IsInvalidWavNameOrType(string wavName, string wavType)
        {
            if (ExcludeAudioBlocks.Any(invalid => wavName.Contains(invalid)))
                return true;

            if (version < 10)
                return !IsValidWavType(wavType);

            if (!string.IsNullOrEmpty(wavType))
                return !IsValidWavType(wavType);

            return !(wavName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                     wavName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if the provided WAV type is valid based on known supported types.
        /// </summary>
        /// <param name="wavType">The WAV type to validate.</param>
        /// <returns>True if the WAV type is valid, otherwise false.</returns>
        private static bool IsValidWavType(string wavType)
        {
            var validWavTypes = new[] { "WAVE", "EVAW", "AIFF", "FFIA" };
            return validWavTypes.Any(validType => validType.Equals(wavType, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            xorReader?.Dispose();
            fileStream?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
