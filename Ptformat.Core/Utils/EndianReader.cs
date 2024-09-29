using System;

namespace Ptformat.Core.Utilities
{
    public static class EndianReader
    {

        /// <summary>
        /// Reads a value from the buffer based on the specified length and offset.
        /// </summary>
        /// <param name="buffer">The byte array containing the data.</param>
        /// <param name="length">The number of bytes to read (1 to 5).</param>
        /// <param name="offset">The starting index within the buffer (default is 0).</param>
        /// <returns>The value read from the buffer as a long.</returns>
        public static long ReadValueFromBytes(byte[] buffer, int length, int offset, bool isBigEndian)
        {
            var segment = new Span<byte>(buffer, offset, length);

            return length switch
            {
                5 => EndianReader.ReadInt64(segment.ToArray(), offset, isBigEndian),
                4 => EndianReader.ReadInt32(segment.ToArray(), offset, isBigEndian),
                3 => EndianReader.ReadInt24(segment.ToArray(), offset, isBigEndian),
                2 => EndianReader.ReadInt16(segment.ToArray(), offset, isBigEndian),
                1 => EndianReader.ReadInt16(segment.ToArray(), offset, isBigEndian),
                _ => throw new ArgumentException("Invalid length specified. Must be between 1 and 5.", nameof(length))
            };
        }

        /// <summary>
        /// Reads a 2-byte integer from a buffer, respecting endianness.
        /// </summary>
        public static int ReadInt16(byte[] buffer, int offset, bool isBigEndian)
        {
            ValidateBuffer(buffer, offset, 2);

            return isBigEndian
                ? (buffer[offset] << 8) | buffer[offset + 1]
                : (buffer[offset + 1] << 8) | buffer[offset];
        }

        /// <summary>
        /// Reads a 3-byte integer from a buffer, respecting endianness.
        /// </summary>
        public static int ReadInt24(byte[] buffer, int offset, bool isBigEndian)
        {
            ValidateBuffer(buffer, offset, 3);

            return isBigEndian
                ? (buffer[offset] << 16) | (buffer[offset + 1] << 8) | buffer[offset + 2]
                : (buffer[offset + 2] << 16) | (buffer[offset + 1] << 8) | buffer[offset];
        }

        /// <summary>
        /// Reads a 4-byte integer from a buffer, respecting endianness.
        /// </summary>
        public static int ReadInt32(byte[] buffer, int offset, bool isBigEndian)
        {
            ValidateBuffer(buffer, offset, 4);

            return isBigEndian
                ? (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3]
                : (buffer[offset + 3] << 24) | (buffer[offset + 2] << 16) | (buffer[offset + 1] << 8) | buffer[offset];
        }

        /// <summary>
        /// Reads a 5-byte long integer from a buffer, respecting endianness.
        /// </summary>
        public static long ReadInt40(byte[] buffer, int offset, bool isBigEndian)
        {
            ValidateBuffer(buffer, offset, 5);

            return isBigEndian
                ? ((long)buffer[offset] << 32) |
                  ((long)buffer[offset + 1] << 24) |
                  ((long)buffer[offset + 2] << 16) |
                  ((long)buffer[offset + 3] << 8) |
                   buffer[offset + 4]
                : ((long)buffer[offset + 4] << 32) |
                  ((long)buffer[offset + 3] << 24) |
                  ((long)buffer[offset + 2] << 16) |
                  ((long)buffer[offset + 1] << 8) |
                   buffer[offset];
        }

        /// <summary>
        /// Reads an 8-byte long integer from a buffer, respecting endianness.
        /// </summary>
        public static long ReadInt64(byte[] buffer, int offset, bool isBigEndian)
        {
            ValidateBuffer(buffer, offset, 8);

            return isBigEndian
                ? ((long)buffer[offset] << 56) |
                  ((long)buffer[offset + 1] << 48) |
                  ((long)buffer[offset + 2] << 40) |
                  ((long)buffer[offset + 3] << 32) |
                  ((long)buffer[offset + 4] << 24) |
                  ((long)buffer[offset + 5] << 16) |
                  ((long)buffer[offset + 6] << 8) |
                   buffer[offset + 7]
                : ((long)buffer[offset + 7] << 56) |
                  ((long)buffer[offset + 6] << 48) |
                  ((long)buffer[offset + 5] << 40) |
                  ((long)buffer[offset + 4] << 32) |
                  ((long)buffer[offset + 3] << 24) |
                  ((long)buffer[offset + 2] << 16) |
                  ((long)buffer[offset + 1] << 8) |
                   buffer[offset];
        }

        /// <summary>
        /// Validates that the buffer has enough length to read the required bytes.
        /// </summary>
        private static void ValidateBuffer(byte[] buffer, int offset, int byteCount)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is outside the bounds of the buffer.");

            if (buffer.Length < offset + byteCount)
                throw new ArgumentException($"Buffer length must be at least {offset + byteCount} to read {byteCount} bytes from the given offset.");
        }
    }
}
