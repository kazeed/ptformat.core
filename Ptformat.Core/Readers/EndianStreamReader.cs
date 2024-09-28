using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Ptformat.Core.Readers
{
    public class EndianStreamReader(Stream stream, bool isBigEndian) : StreamReader(stream)
    {
        private long previousPosition;
        public bool IsBigEndian => isBigEndian;

        /// <summary>
        /// Reads a specified number of bytes (2, 3, 4, 5, or 8), applies endianness transformations, and returns the result as a long.
        /// </summary>
        /// <param name="length">The number of bytes to read (valid values: 2, 3, 4, 5, or 8).</param>
        /// <returns>The resulting long value after applying endianness transformations.</returns>
        public long Read(int length)
        {
            if (length != 2 && length != 3 && length != 4 && length != 5 && length != 8)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be 2, 3, 4, 5, or 8 bytes.");

            try
            {
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
                        result = result << 8 | buffer[i];
                    }
                }
                else
                {
                    for (int i = length - 1; i >= 0; i--)
                    {
                        result = result << 8 | buffer[i];
                    }
                }

                return result;
            }
            catch (EndOfStreamException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Jumps forward in the stream to the position where the needle is first found.
        /// </summary>
        /// <param name="needle">The byte sequence to search for.</param>
        /// <returns>True if the needle is found; otherwise, false.</returns>
        public bool JumpTo(byte[] needle)
        {
            if (needle == null || needle.Length == 0)
                throw new ArgumentException("Needle cannot be null or empty.", nameof(needle));

            if (BaseStream.Position + needle.Length >= BaseStream.Length)
                return false;

            previousPosition = BaseStream.Position; // Save the current position before jumping

            var buffer = new byte[needle.Length];

            while (BaseStream.Position + needle.Length <= BaseStream.Length)
            {
                var bytesRead = BaseStream.Read(buffer, 0, needle.Length);
                if (bytesRead != needle.Length)
                    return false;

                if (buffer.AsSpan().SequenceEqual(needle))
                {
                    return true;
                }

                BaseStream.Position = BaseStream.Position - needle.Length + 1; // Move one byte forward
            }

            BaseStream.Position = previousPosition; // Restore the previous position if not found
            return false;
        }

        /// <summary>
        /// Jumps backward in the stream to the position where the needle is first found.
        /// </summary>
        /// <param name="needle">The byte sequence to search for.</param>
        /// <returns>True if the needle is found; otherwise, false.</returns>
        public bool JumpBack(byte[] needle)
        {
            if (needle == null || needle.Length == 0)
                throw new ArgumentException("Needle cannot be null or empty.", nameof(needle));

            if (BaseStream.Position <= 0 || BaseStream.Position + needle.Length >= BaseStream.Length)
                return false;

            previousPosition = BaseStream.Position; // Save the current position before jumping

            var buffer = new byte[needle.Length];

            while (BaseStream.Position > 0)
            {
                BaseStream.Position--;
                var bytesRead = BaseStream.Read(buffer, 0, needle.Length);
                BaseStream.Position -= bytesRead; // Return back after reading

                if (bytesRead != needle.Length)
                    return false;

                if (buffer.AsSpan().SequenceEqual(needle))
                {
                    BaseStream.Position++; // Adjust to the exact start of the found needle
                    return true;
                }
            }

            BaseStream.Position = previousPosition; // Restore the previous position if not found
            return false;
        }

        /// <summary>
        /// Returns the stream to the last saved position before the previous jump operation.
        /// </summary>
        public void ReturnToPreviousPosition()
        {
            BaseStream.Position = previousPosition;
        }
    }
}