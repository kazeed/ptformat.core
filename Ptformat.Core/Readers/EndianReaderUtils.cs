using System;
using System.IO;

public class EndianStreamReader : StreamReader
{
    public bool IsBigEndian { get; }

    public EndianStreamReader(Stream stream, bool isBigEndian) : base(stream)
    {
        IsBigEndian = isBigEndian;
    }

    /// <summary>
    /// Reads a specified number of bytes (2, 3, 4, 5, or 8), applies endianness transformations, and returns the result as a long.
    /// </summary>
    /// <param name="length">The number of bytes to read (valid values: 2, 3, 4, 5, or 8).</param>
    /// <returns>The resulting long value after applying endianness transformations.</returns>
    public long Read(int length)
    {
        if (length != 2 && length != 3 && length != 4 && length != 5 && length != 8)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be 2, 3, 4, 5, or 8 bytes.");

        byte[] buffer = new byte[length];
        int bytesRead = BaseStream.Read(buffer, 0, length);

        if (bytesRead != length)
            throw new EndOfStreamException($"Reached the end of the stream unexpectedly while attempting to read {length} bytes.");

        long result = 0;

        // Handle endianness
        if (IsBigEndian)
        {
            for (int i = 0; i < length; i++)
            {
                result = (result << 8) | buffer[i];
            }
        }
        else
        {
            for (int i = length - 1; i >= 0; i--)
            {
                result = (result << 8) | buffer[i];
            }
        }

        return result;
    }
}
