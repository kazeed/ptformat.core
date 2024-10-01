using System;

namespace Ptformat.Core.Parsers
{
    public class PtsParsingException(string message, int offset = 0) : Exception(message)
    {
        public int Offset { get; } = offset;
    }
}
