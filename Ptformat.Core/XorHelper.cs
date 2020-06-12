using System;
using System.IO;

namespace Ptformat.Core
{
    public static class XorHelper
    {
        public const byte Comparer = 0xff;

        public static byte[] Xor(byte[] input, byte[] key, byte type = 0x01)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // perform XOR operation of key with every caracter in string
            using var inputStream = new MemoryStream(input);
            using var outputStream = new MemoryStream();
            while (inputStream.Position < inputStream.Length)
            {
                var i = inputStream.Position;
                var b = inputStream.ReadByte();
                var idx = type == 0x01 ? i & Comparer : (i << 12) & Comparer;
                var unxor = (byte)(b ^ key[idx % key.Length]);
                outputStream.WriteByte(unxor);
            }

            return outputStream.ToArray();
        }

        public static byte[] GenerateKey(byte delta)
        {
            const int length = 256;
            byte[] key = new byte[length];
            for (var i = 0; i < length; i++)
            {
                key[i] = (byte)((i * delta) & XorHelper.Comparer);
            }

            return key;
        }

        public static byte GenerateDelta(byte xorvalue, byte multiplier, bool negative)
        {
            for (var i = 0; i < 256; i++)
            {
                if (((i * multiplier) & Comparer) == xorvalue)
                {
                    return (byte)(negative ? i * (-1) : i);
                }
            }

            // Should not occur
            throw new Exception("Unable to generate delta for XOR encryption");
        }
    }
}
