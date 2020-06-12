using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ptformat.Core
{
    public static class XorHelper
    {
        public const byte PTVersion5or9 = 0x01;
        public const byte PTVersion10or12 = 0x05;
        public const byte Comparer = 0xff;

        public static string NewXor(byte[] input, byte[] key)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string outputString = string.Empty;

            // perform XOR operation of key with every caracter in string
            for (int i = 0; i < input.Length; i++)
            {
                outputString += char.ToString((char)(input[i] ^ key[i % key.Length]));
            }

            return outputString;
        }

        public static string Xor(byte[] input, byte[] key, byte type)
        {
            var decrypted = input.Select(b =>
                {
                    var i = Array.IndexOf(input, b);
                    var idx = (type == PTVersion5or9) ? i & Comparer : (i >> 12) & Comparer;
                    return b ^ key[idx];
                }).Cast<byte>().ToArray();

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
