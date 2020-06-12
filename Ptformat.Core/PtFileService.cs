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
                        delta = XorHelper.GenerateDelta(xorvalue, 53, false);
                        break;
                    case 5:
                        delta = XorHelper.GenerateDelta(xorvalue, 11, true);
                        break;
                    default:
                        return null;
                }

                var key = XorHelper.GenerateKey(delta);

                var encrypted = file.Skip(20).ToArray();
                var decrypted = XorHelper.Xor(encrypted, key, type);

                return unencrypted.Concat(decrypted).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt file", ex);
            }
        }
    }
}
