using System;

namespace Ptformat.Core.Parsers
{
    public class PtsParsingException : Exception
    {
        public int Offset { get; }

        public PtsParsingException(string message, int offset = 0) : base(message)
        {
            Offset = offset;
        }
    }
}
