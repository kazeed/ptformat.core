using Ptformat.Core.Utilities;
using System;
using System.Linq;
using System.Text;

namespace Ptformat.Core.Parsers
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
    }
}

