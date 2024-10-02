using Ptformat.Core.Extensions;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Ptformat.Core.Parsers
{
    public class AudioParser : IListParser<AudioTrack>
    {
        public List<AudioTrack> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var audioTracks = new List<AudioTrack>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek to inspect the block without removing it
                if (block.ContentType == ContentType.WavListFull)
                {
                    blocks.Dequeue(); // Dequeue the block after processing it

                    // Extract the number of wavs
                    var nwavs = EndianReader.ReadInt32(rawFile, (int)block.Offset + 2, isBigEndian);
                    var wavFiles = new List<AudioRef>();

                    // Process child blocks with wav file information
                    foreach (var child in block.Children.Where(c => c.ContentType == ContentType.WavNames))
                    {
                        var pos = (int)child.Offset + 11;

                        for (int i = 0, n = 0; pos < child.Offset + child.Size && n < nwavs; i++)
                        { 
                            var wavName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);
                            var wavType = ParserUtils.ParseASCIIString(rawFile, ref pos, 4);
                            pos += 5;

                            if (!ParserUtils.IsInvalidWavNameOrType(wavName, wavType))
                            {
                                var audioRef = new AudioRef(n, wavName);
                                audioRef.AddLength(block.Children); // Assuming AddLength is the extension method you created
                                wavFiles.Add(audioRef);
                                n++;
                            }
                        }
                    }

                    // Extract the track name (if available)
                    var trackName = wavFiles.FirstOrDefault()?.Filename ?? "Unknown Audio Track";

                    // Create AudioTrack with the extracted name and list of wav files
                    var audioTrack = new AudioTrack(trackName) { AudioFiles = wavFiles };
                    audioTracks.Add(audioTrack);
                }
                else
                {
                    break; // Exit if the block isn't relevant to audio parsing
                }
            }

            return audioTracks;
        }
    }
}