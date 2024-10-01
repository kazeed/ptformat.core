using Ptformat.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptformat.Core.Parsers
{
    public static class ParserUtils
    {
        private static readonly string[] InvalidNames = [".grp", "Audio Files", "Fade Files"];
        private static readonly string[] ValidWavTypes = ["WAVE", "EVAW", "AIFF", "FFIA"];
        
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
        /// <param name="data">The byte array containing the file data.</param>
        /// <param name="position">The starting position in the data array to parse the string.</param>
        /// <param name="isBigEndian">Indicates if the data is in big-endian format.</param>
        /// <returns>The parsed string.</returns>
        public static string ParseString(byte[] data, ref int position, bool isBigEndian)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (position + 4 >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of data bounds.");

            // Read the length of the string (4 bytes) using EndianReader
            var length = EndianReader.ReadInt32(data, position, isBigEndian);

            // Move the position forward by 4 bytes to read the actual string content
            position += 4;

            // Ensure the string length is within the bounds
            if (position + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(position), "String length exceeds data bounds.");

            // Extract the string content
            var content = Encoding.ASCII.GetString(data, position, length).TrimEnd('\0');

            // Update the position by the length of the string
            position += length;

            return content;
        }
    }
}
}
