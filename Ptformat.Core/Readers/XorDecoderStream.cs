using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Ptformat.Core.Readers
{
    public class  XorDecoderStream : MemoryStream
    {
        private readonly byte[] xorTable = new byte[256];
        private readonly byte xorType;
        private readonly byte xorValue;
        private readonly long initialPosition;
        private readonly ILogger<XorDecoderStream> logger;

        public XorDecoderStream(byte[] buffer, ILogger<XorDecoderStream> logger) : base(buffer)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            initialPosition = base.Position;

            try
            {
                // Read the first 20 bytes to get XOR details
                var header = new byte[20];
                base.Read(header, 0, 20);

                xorType = header[0x12];
                xorValue = header[0x13];

                logger.LogInformation("XOR Type: {xorType}, XOR Value: {xorValue}", xorType, xorValue);

                // Generate XOR table
                var xorDelta = xorType switch
                {
                    0x01 => GenerateXorDelta(xorValue, 53, false),
                    0x05 => GenerateXorDelta(xorValue, 11, true),
                    _ => throw new InvalidDataException($"Unknown XOR type: {xorType}"),
                };

                // Build XOR table cell
                for (int i = 0; i < 256; i++)
                {
                    xorTable[i] = (byte)(i * xorDelta & 0xff);
                }

                logger.LogInformation("XOR Table generated successfully.");
                base.Position = initialPosition;
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is OutOfMemoryException || ex is IOException || ex is InvalidDataException)
            {
                logger.LogError(ex, "An error occurred during initialization: {Message}", ex.Message);
                throw;
            }
        }

        public byte[] Decode()
        {
            try
            {
                var result = base.ToArray();

                for (long i = 0; i < result.Length; i++)
                {
                    int xorIndex = xorType == 0x01 ? (int)(i & 0xff) : (int)(i >> 12 & 0xff);
                    result[i] ^= xorTable[xorIndex];
                }

                logger.LogInformation("ReadToEndAsync completed successfully for content with length {length}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during ReadToEndAsync: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generates the XOR delta value based on the given parameters.
        /// </summary>
        /// <param name="xorValue">The XOR value.</param>
        /// <param name="multiplier">The multiplier used in delta calculation.</param>
        /// <param name="negative">Determines whether the result should be negative.</param>
        /// <returns>The generated XOR delta value.</returns>
        private byte GenerateXorDelta(byte xorValue, byte multiplier, bool negative)
        {
            for (byte i = 0; i <= byte.MaxValue; i++)
            {
                if ((i * multiplier & 0xff) == xorValue)
                    return (byte)(negative ? i * -1 : i);
            }
            logger.LogWarning("No valid XOR delta found for value: {xorValue}", xorValue);
            return 0;
        }
    }
}