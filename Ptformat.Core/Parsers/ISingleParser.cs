using PtInfo.Core.Model;
using System.Collections.Generic;

namespace PtInfo.Core.Parsers
{
    public interface ISingleParser<T>
    {
        /// <summary>
        /// Parses a queue of blocks and extracts relevant data.
        /// </summary>
        /// <param name="blocks">The queue of blocks to parse.</param>
        /// <param name="rawFile">The raw file data to assist in parsing.</param>
        /// <param name="isBigEndian">Specifies if the file is in big-endian format.</param>
        /// <returns>Returns a list of parsed results of type T.</returns>
        T Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian);
    }
}