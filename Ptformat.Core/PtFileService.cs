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

        private const byte PTVersion_5_9 = 0x01;
        private const byte PTVersion_10_12 = 0x05;
        private const byte Comparer = 0xff;

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
                var unencrypted = new byte[20];
                await file.ReadAsync(unencrypted, 0, 20).ConfigureAwait(false);
                var xor = new XorParams { Type = unencrypted[18], Value = unencrypted[19] };
                xor.Delta = this.GenerateDelta(xor);
                xor.Key = this.GenerateKey(xor);

                // Ensure we're at beginning of encryption
                file.Seek(20, SeekOrigin.Begin);

                var encrypted = new byte[file.Length - 20];
                await file.ReadAsync(encrypted, 20, (int)file.Length - 20);
                var decrypted = encrypted.Select(b =>
                {
                    var i = Array.IndexOf(encrypted, b);
                    var idx = (xor.Type == PTVersion_5_9) ? i & Comparer : (i >> 12) & Comparer;
                    return b ^ xor.Key[idx];
                }).Cast<byte>().ToArray();

                return new MemoryStream(decrypted);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt file", ex);
            }
        }

        private byte[] GenerateKey(XorParams xor)
        {
            byte[] key = new byte[xor.Length];
            for (var i = 0; i < xor.Length; i++)
            {
                key[i] = (byte)((i * xor.Delta) & Comparer);
            }

            return key;
        }

        private byte GenerateDelta(XorParams xor)
        {
            var multiplier = (xor.Value == PTVersion_5_9 ? 53 : (xor.Value == PTVersion_10_12 ? 11 : -1));
            var negative = (multiplier == 53 ? true : false);
            if (multiplier == -1)
            {
                throw new Exception("Unable to determine PT file version");
            }
            
            for (var i = 0; i < 256; i++)
            {
                if (((i * multiplier) & Comparer) == xor.Value)
                {
                    return (byte)(negative ? i * (-1) : i);
                }
            }

            throw new Exception("Unable to generate delta for XOR encryption");
        }
    }

    internal class XorParams
    {
        public int Type { get; set; }

        public byte Value { get; set; }

        public int Length => 256;

        public byte Delta { get; set; }

        public byte[] Key { get; set; }
    }
}
