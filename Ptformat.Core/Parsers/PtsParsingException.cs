﻿using System;

namespace PtInfo.Core.Parsers
{
    public class PtsParsingException(string message, int offset = 0) : Exception(message)
    {
        public int Offset { get; } = offset;
    }
}
