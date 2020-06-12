using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ptformat.Core
{
    public static class XorHelper
    {
        public const byte PTVersion5or9 = 0x01;
        public const byte PTVersion10or12 = 0x05;
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
    }
}
