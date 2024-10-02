using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Ptformat.Core.Parsers
{
    public class TrackParser(ILogger<TrackParser> logger, IListParser<Region> audioRegionParser, IListParser<Region> compoundRegionParser, IListParser<Region> midiRegionParser) : IListParser<Track>
    {
        private readonly ILogger<TrackParser> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IListParser<Region> audioRegionParser = audioRegionParser ?? throw new ArgumentNullException(nameof(audioRegionParser));
        private readonly IListParser<Region> compoundRegionParser = compoundRegionParser ?? throw new ArgumentNullException(nameof(compoundRegionParser));
        private readonly IListParser<Region> midiRegionParser = midiRegionParser ?? throw new ArgumentNullException(nameof(midiRegionParser));

        public List<Track> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            // First parse all regions
            var audioRegions = audioRegionParser.Parse(blocks, rawFile, isBigEndian);
            var compoundRegions = compoundRegionParser.Parse(blocks, rawFile, isBigEndian);
            var midiRegions = midiRegionParser.Parse(blocks, rawFile, isBigEndian);

            var tracks = new List<Track>();

            // Now parse audio and midi tracks and map the appropriate regions
            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek at the block

                if (block.ContentType == ContentType.AudioTracks)
                {
                    blocks.Dequeue(); // Consume the block
                    var audioTracks = ParseAudioTracks(block, rawFile, isBigEndian);
                    tracks.AddRange(MapRegionsToTracks(audioTracks, audioRegions, compoundRegions));
                }
                else if (block.ContentType == ContentType.MidiTrackFullList)
                {
                    blocks.Dequeue(); // Consume the block
                    var midiTracks = ParseMidiTracks(block, rawFile, isBigEndian);
                    tracks.AddRange(MapRegionsToTracks(midiTracks, midiRegions, compoundRegions));
                }
                else
                {
                    // Exit if the block isn't related to tracks
                    break;
                }
            }

            return tracks;
        }

        /// <summary>
        /// Maps regions to the corresponding tracks.
        /// </summary>
        private IEnumerable<Track> MapRegionsToTracks(IEnumerable<Track> tracks, List<Region> specificRegions, List<Region> compoundRegions)
        {
            foreach (var track in tracks)
            {
                // Assign specific regions (audio or midi) based on track type
                track.Regions = specificRegions
                    .Where(r => r.Name.StartsWith(track.Name, StringComparison.OrdinalIgnoreCase)) // Example condition
                    .ToList();

                // Optionally, assign compound regions if relevant for the track
                track.Regions.AddRange(compoundRegions
                    .Where(r => r.Name.StartsWith(track.Name, StringComparison.OrdinalIgnoreCase))); // Example condition

                logger.LogInformation("Mapped {regionCount} regions to track: {trackName}", track.Regions.Count, track.Name);
            }

            return tracks;
        }

        /// <summary>
        /// Parses audio tracks from the given block.
        /// </summary>
        private static List<AudioTrack> ParseAudioTracks(Block block, byte[] rawFile, bool isBigEndian)
        {
            var audioTracks = new List<AudioTrack>();

            foreach (var child in block.Children.Where(c => c.ContentType == ContentType.AudioTrackNameNumber))
            {
                var pos = (int)child.Offset + 2;
                var trackName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);
                pos += 5;

                var nch = EndianReader.ReadInt32(rawFile, pos, isBigEndian);
                pos += 4;
                var channels = new List<int>();

                for (var i = 0; i < nch; i++)
                {
                    channels.Add(EndianReader.ReadInt16(rawFile, pos, isBigEndian));
                    pos += 2;
                }

                var audioTrack = new AudioTrack(trackName)
                {
                    Channels = channels
                };

                audioTracks.Add(audioTrack);
            }

            return audioTracks;
        }

        /// <summary>
        /// Parses MIDI tracks from the given block.
        /// </summary>
        private static List<MidiTrack> ParseMidiTracks(Block block, byte[] rawFile, bool isBigEndian)
        {
            var midiTracks = new List<MidiTrack>();
            var trackIndex = 0;

            foreach (var child in block.Children.Where(c => c.ContentType == ContentType.MidiTrackNameNumber))
            {
                var pos = (int)child.Offset + 4;
                var trackName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);
                pos += 22;

                var midiTrack = new MidiTrack(trackName)
                {
                    Index = trackIndex
                };

                midiTracks.Add(midiTrack);
                trackIndex++;
            }

            return midiTracks;
        }
    }
}