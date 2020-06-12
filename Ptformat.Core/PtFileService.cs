// <copyright file="PtFileService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ptformat.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PtFileService
    {
        private const string InvalidPTFile = "Invalid PT file";

        public byte[] DecryptFile(byte[] file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.Length < 20)
            {
                throw new ArgumentException(InvalidPTFile);
            }

            try
            {
                // First 20 bytes unencrypted
                var unencrypted = file.Take(20).ToArray();
                var type = unencrypted[18];
                var xorvalue = unencrypted[19];

                // xor_type 0x01 = ProTools 5, 6, 7, 8 and 9
                // xor_type 0x05 = ProTools 10, 11, 12
                byte delta;
                switch (type)
                {
                    case 1:
                        delta = GenerateDelta(xorvalue, 53, false);
                        break;
                    case 5:
                        delta = GenerateDelta(xorvalue, 11, true);
                        break;
                    default:
                        return null;
                }

                var key = this.GenerateKey(delta);

                var encrypted = file.Skip(20).ToArray();
                var decrypted = XorHelper.Xor(encrypted, key, type);

                return decrypted;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt file", ex);
            }
        }

        private byte[] GenerateKey(byte delta)
        {
            const int length = 256;
            byte[] key = new byte[length];
            for (var i = 0; i < length; i++)
            {
                key[i] = (byte)((i * delta) & XorHelper.Comparer);
            }

            return key;
        }

        private byte GenerateDelta(byte xorvalue, byte multiplier, bool negative)
        {
            for (var i = 0; i < 256; i++)
            {
                if (((i * multiplier) & 0xff) == xorvalue)
                {
                    return (byte)(negative ? i * (-1) : i);
                }
            }

            // Should not occur
            throw new Exception("Unable to generate delta for XOR encryption");
        }
    }
}
