using Ptformat.Core.Extensions;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ptformat.Core.Parsers
{
    public class AudioParser : IAudioParser
    {
        /// <summary>
        /// Parses wavlist blocks in the file data to extract audio file information.
        /// </summary>
        public List<AudioRef> ParseAudio(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var wavFiles = new List<AudioRef>();

            while (blocks.Count > 0)
            {
                var block = blocks.Dequeue();
                if (block.ContentType != ContentType.WavListFull)
                    continue;

                var nwavs = EndianReader.ReadInt32(rawFile, (int)block.Offset + 2, isBigEndian);

                // Process child blocks for WAV names
                foreach (var child in block.Children.Where(c => c.ContentType == ContentType.WavNames))
                {
                    var pos = (int)child.Offset + 11;

                    for (int i = 0, n = 0; (pos < child.Offset + child.Size) && (n < nwavs); i++)
                    {
                        var wavName = ParserUtils.ParseString(block.RawData, ref pos, isBigEndian);

                        var wavType = Encoding.ASCII.GetString(rawFile, pos, 4);
                        pos += 9;

                        if (ParserUtils.IsInvalidWavNameOrType(wavName, wavType))
                            continue;

                        wavFiles.Add(new AudioRef(n, wavName));
                        n++;
                    }
                }

                wavFiles = wavFiles.Select(aRef => aRef.AddLength([.. blocks])).ToList();
            }

            return wavFiles;
        }
    }

}
