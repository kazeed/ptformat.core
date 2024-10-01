using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Ptformat.Core.Extensions
{
    public static  class ByteArrayExtensions
    {
        private const int FMARK = 0x3F;
        /// <summary>
        /// Parses fields from the raw block data using 0x3F as the field separator.
        /// </summary>
        /// <param name="rawData">The raw data of the block.</param>
        /// <returns>A list of fields as strings.</returns>
        public static List<string> ParseFields(this byte[] rawData)
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
    }
}
