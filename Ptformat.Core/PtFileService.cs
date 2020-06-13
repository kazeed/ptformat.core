// <copyright file="PtFileService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Ptformat.Core.Model;
using Ptformat.Core.Readers;

namespace Ptformat.Core
{
    public class PtFileService
    {
        private const string InvalidPTFile = "Invalid PT file";
        private const char ZMark = '\x5a';
        private const long ZeroTicks = 0xe8d4a51000L;
        private const int MaxContentType = 0x3000;
        private const int MaxChanelsPerTrack = 8;

        private readonly byte[] bitCode = new byte[] { 0x2F, 0x2B };

        private byte[] decoded;
        private long length;
        private byte product;
        private long targetRate;
        private float rateFactor;
        private bool isBigEndian;

        public List<Wave> AudioFiles => new List<Wave>();

        public List<Region> Regions => new List<Region>();

        public List<Region> MidiRegions => new List<Region>();

        public List<Track> Tracks => new List<Track>();

        public List<Track> MidiTracks => new List<Track>();

        public List<Block> Blocks => new List<Block>();

        public byte Version { get; set; }

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

                decoded = unencrypted.Concat(decrypted).ToArray();

                return decoded;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to decrypt file", ex);
            }
        }

        public byte GetVersion()
        {
            var b = new Block();

            if (decoded[0] != 0x03 && decoded.FoundAt(0x100, bitCode) != 1) throw new Exception("Cannot calculate version");

            this.isBigEndian = decoded[0x11] != 0;

            if (ParseBlock(0x1f, 0) is Block b)
            {
                if (b.ContentType == 0x0003)
                {
                    // old
                    var skip = ParseString(b.Offset + 3).Length + 8;
                    Version = (byte)EndianReader.Read4(decoded.GetRange(b.Offset + 3 + skip, 1), isBigEndian);
                    return Version;
                }
                else if (b.content_type == 0x2067)
                {
                    // new
                    Version = (byte)EndianReader.Read4(decoded.GetRange(b.Offset + 20, 1), isBigEndian);
                    return Version;
                }
                return failed;
            }
            else
            {
                Version = decoded[0x40];
                if (Version == 0)
                {
                    Version = decoded[0x3d];
                }

                if (Version == 0)
                {
                    Version = (byte)(decoded[0x3a] + 2);
                }

                return Version;
            }
            /*
             * bool
            PTFFormat::parseVersion() {
	            bool failed = true;
	            struct block_t b;

	            if (!parse_block_at(0x1f, &b, NULL, 0)) {
		            Version = decoded[0x40];
		            if (Version == 0) {
			            Version = decoded[0x3d];
		            }
		            if (Version == 0) {
			            Version = decoded[0x3a] + 2;
		            }
		            if (Version != 0)
			            failed = false;
		            return failed;
	            } else {
		            if (b.content_type == 0x0003) {
			            // old
			            uint16_t skip = parsestring(b.offset + 3).size() + 8;
			            Version = u_endian_read4(&decoded[b.offset + 3 + skip], is_bigendian);
			            failed = false;
		            } else if (b.content_type == 0x2067) {
			            // new
			            Version = 2 + u_endian_read4(&decoded[b.offset + 20], is_bigendian);
			            failed = false;
		            }
		            return failed;
	            }
}*/
        }

        private Block ParseBlock(long pos, int level, Block parent = null)
        {
            var childjump = 0;
            var max = (long)this.decoded.Length;

            if (decoded[pos] != ZMark) return null;

            if (parent != null)
            {
                max = parent.Size + parent.Offset;
            }

            var b = new Block
            {
                ZMark = (byte)ZMark,
                Type = EndianReader.Read2(decoded.GetRange(pos + 1, 2), isBigEndian),
                Size = EndianReader.Read4(decoded.GetRange(pos + 3, 4), isBigEndian),
                ContentType = EndianReader.Read2(decoded.GetRange(pos + 7, 2), isBigEndian),
                Offset = pos + 7
            };

            if (b.Size + b.Offset > max) return null;

            if (b.Type == 0xff00) return null;

            for (var i = 1; (i < b.Size) && (pos + i + childjump < max); i += childjump != 0 ? childjump : 1)
            {
                int p = (int)pos + i;
                childjump = 0;
                if (ParseBlock(p, level + 1, b) is Block child)
                {
                    b.Children.Add(child);
                    childjump = child.Size + 7;
                }
            }

            return b;
		}

        private string ParseString(long pos)
        {
            var length = EndianReader.Read4(decoded.GetRange(pos, 4), isBigEndian));
            return decoded.GetRange(pos + 4, length).AsString();
        }
    }
}
