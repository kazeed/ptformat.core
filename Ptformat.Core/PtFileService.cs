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

        public async Task<Stream> DecryptFile(Stream file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // Ensure we're at the beginning
            file.Seek(0, SeekOrigin.Begin);

            if (file.Length < 20)
            {
                throw new ArgumentException(InvalidPTFile);
            }

            try
            {
                // First 20 bytes unencrypted
                var unencrypted = new byte[20];
                await file.ReadAsync(unencrypted, 0, 20).ConfigureAwait(false);
                var type = unencrypted[18];
                var xorvalue = unencrypted[19];
                var delta = this.GenerateDelta(type);
                var key = this.GenerateKey(delta);

                // Ensure we're at beginning of encryption
                file.Seek(20, SeekOrigin.Begin);

                var encrypted = new byte[file.Length - 20];
                await file.ReadAsync(encrypted, 0, (int)file.Length - 20);
                var decrypted = XorHelper.Xor(encrypted, key, type);

                return new MemoryStream(Encoding.UTF8.GetBytes(decrypted));
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

        private byte GenerateDelta(byte xorvalue)
        {
            var multiplier = xorvalue == XorHelper.PTVersion5or9 ? 53 : (xorvalue == XorHelper.PTVersion10or12 ? 11 : -1);
            var negative = multiplier == 53;
            if (multiplier == -1)
            {
                throw new Exception("Unable to determine PT file version");
            }

            for (var i = 0; i < 256; i++)
            {
                if (((i * multiplier) & XorHelper.Comparer) == xorvalue)
                {
                    return (byte)(negative ? i * (-1) : i);
                }
            }

            throw new Exception("Unable to generate delta for XOR encryption");
        }
    }
}
