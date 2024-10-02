using PtInfo.Core.Model;
using PtInfo.Core.Utilities;
using System;
using System.Linq;
using System.Text;

namespace PtInfo.Core.Parsers
{
    public static class ParserUtils
    {
        private static readonly string[] InvalidNames = [".grp", "Audio Files", "Fade Files"];
        private static readonly string[] ValidWavTypes = ["WAVE", "EVAW", "AIFF", "FFIA"];
        private const byte ZMARK = 0x5A;

        /// <summary>
        /// Determines if a WAV name or type is invalid based on the file version and expected formats.
        /// </summary>
        public static bool IsInvalidWavNameOrType(string wavName, string wavType) =>
            InvalidNames.Any(invalid => wavName.Contains(invalid)) ||
            (!string.IsNullOrEmpty(wavType) && !ValidWavTypes.Contains(wavType)) ||
            !(wavName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || wavName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Parses a string from the specified position in the given data array.
        /// The method reads the length of the string first and then extracts the string content.
        /// </summary>
        /// <param name="bufffer">The byte array containing the file data.</param>
        /// <param name="position">The starting position in the data array to parse the string.</param>
        /// <param name="isBigEndian">Indicates if the data is in big-endian format.</param>
        /// <returns>The parsed string.</returns>
        public static string ParseString(byte[] bufffer, ref int position, bool isBigEndian)
        {
            ArgumentNullException.ThrowIfNull(bufffer);
            if (position + 4 >= bufffer.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of data bounds.");

            // Read the length of the string (4 bytes) using EndianReader
            var length = EndianReader.ReadInt32(bufffer, position, isBigEndian);

            // Move the position forward by 4 bytes to read the actual string content
            position += 4;

            // Ensure the string length is within the bounds
            if (position + length > bufffer.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "String length exceeds data bounds.");

            // Extract the string content
            var content = Encoding.ASCII.GetString(bufffer, position, length).TrimEnd('\0');

            // Update the position by the length of the string
            position += length;

            return content;
        }

        public static string ParseASCIIString(byte[] buffer, ref int position, int length)
        {
            var result = Encoding.ASCII.GetString(buffer, position, length);
            position += 9;
            return result;
        }

        /// <summary>
        /// Reads the content of a block from the given start offset to the next ZMARK (0x5A) or end of the stream.
        /// </summary>
        public static byte[] ReadBlockContent(byte[] buffer, int startOffset)
        {
            int endOffset = startOffset;
            while (endOffset < buffer.Length && buffer[endOffset] != ZMARK)
            {
                endOffset++;
            }

            var blockContent = new byte[endOffset - startOffset];
            Array.Copy(buffer, startOffset, blockContent, 0, blockContent.Length);

            return blockContent;
        }

        /// <summary>
        /// Parses region metadata from the specified position in the file data.
        /// This includes the start, offset, and length of the region.
        /// </summary>
        /// <param name="fileData">The raw file data.</param>
        /// <param name="pos">The current position in the file data, updated by reference.</param>
        /// <param name="isBigEndian">Determines if the file uses big-endian encoding.</param>
        /// <returns>An instance of RegionMetadata representing the extracted values.</returns>
        public static RegionMetadata ParseRegionMetadata(byte[] fileData, ref int pos, bool isBigEndian)
        {
            byte offsetBytes, lengthBytes, startBytes;

            // Determine the byte sizes based on endianness
            if (isBigEndian)
            {
                offsetBytes = (byte)((fileData[pos + 4] & 0xF0) >> 4);
                lengthBytes = (byte)((fileData[pos + 3] & 0xF0) >> 4);
                startBytes = (byte)((fileData[pos + 2] & 0xF0) >> 4);
            }
            else
            {
                offsetBytes = (byte)((fileData[pos + 1] & 0xF0) >> 4);
                lengthBytes = (byte)((fileData[pos + 2] & 0xF0) >> 4);
                startBytes = (byte)((fileData[pos + 3] & 0xF0) >> 4);
            }

            // Read the offset, length, and start using a helper method
            long offset = ReadValueFromBytes(fileData, offsetBytes, ref pos, isBigEndian);
            long length = ReadValueFromBytes(fileData, lengthBytes, ref pos, isBigEndian);
            long start = ReadValueFromBytes(fileData, startBytes, ref pos, isBigEndian);

            // Return the parsed RegionMetadata
            return new RegionMetadata(start, offset, length);
        }

        /// <summary>
        /// Reads a value of the specified length from the byte array and moves the position forward.
        /// </summary>
        /// <param name="buffer">The byte array containing the data.</param>
        /// <param name="length">The number of bytes to read (1 to 5).</param>
        /// <param name="pos">The current position, updated by reference.</param>
        /// <param name="isBigEndian">Indicates whether the data is in big-endian format.</param>
        /// <returns>The extracted value as a long.</returns>
        private static long ReadValueFromBytes(byte[] buffer, int length, ref int pos, bool isBigEndian)
        {
            // Ensure the length is within a valid range
            if (length < 1 || length > 5)
                throw new ArgumentException("Invalid length specified. Must be between 1 and 5.", nameof(length));

            // Read the value based on the length and endianness
            var value = length switch
            {
                5 => EndianReader.ReadInt64(buffer, pos, isBigEndian),
                4 => EndianReader.ReadInt32(buffer, pos, isBigEndian),
                3 => EndianReader.ReadInt24(buffer, pos, isBigEndian),
                2 => EndianReader.ReadInt16(buffer, pos, isBigEndian),
                1 => buffer[pos],
                _ => throw new ArgumentOutOfRangeException(nameof(length), "Unexpected length for region metadata."),
            };

            // Move the position forward
            pos += length;
            return value;
        }
    }
}

