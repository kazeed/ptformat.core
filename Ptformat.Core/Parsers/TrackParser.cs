using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ptformat.Core.Parsers
{
    public class TrackParser(ILogger<TrackParser> logger, AudioRegionParser audioRegionParser, MidiRegionParser midiRegionParser, CompoundRegionParser compoundRegionParser) : IPtParser<Track>
    {
        public List<Track> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            // Parse all types of regions
            var audioRegions = audioRegionParser.Parse(blocks, rawFile, isBigEndian);
            var midiRegions = midiRegionParser.Parse(blocks, rawFile, isBigEndian);
            var compoundRegions = compoundRegionParser.Parse(blocks, rawFile, isBigEndian);

            // Combine all regions into a single list
            var allRegions = new List<Region>();
            allRegions.AddRange(audioRegions);
            allRegions.AddRange(midiRegions);
            allRegions.AddRange(compoundRegions);

            // Parse tracks and map regions to them
            var tracks = ParseTracks(blocks, rawFile, isBigEndian);
            
            return MapRegionsToTracks(tracks, allRegions);
        }

        /// <summary>
        /// Parses both audio and MIDI tracks from the blocks queue.
        /// </summary>
        private static List<Track> ParseTracks(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var tracks = new List<Track>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek at the block

                if (block.ContentType == ContentType.AudioTracks)
                {
                    blocks.Dequeue(); // Consume the block
                    var audioTracks = ParseAudioTracks(block, rawFile, isBigEndian);
                    tracks.AddRange(audioTracks); // Add parsed audio tracks
                }
                else if (block.ContentType == ContentType.MidiTrackFullList)
                {
                    blocks.Dequeue(); // Consume the block
                    var midiTracks = ParseMidiTracks(block, rawFile, isBigEndian);
                    tracks.AddRange(midiTracks); // Add parsed MIDI tracks
                }
                else
                {
                    break; // Exit if the block isn't related to tracks
                }
            }

            return tracks;
        }

        /// <summary>
        /// Maps regions to the corresponding tracks.
        /// </summary>
        private List<Track> MapRegionsToTracks(List<Track> tracks, List<Region> regions)
        {
            foreach (var track in tracks)
            {
                // Assign regions based on their name or other properties (you can refine this condition)
                track.Regions = regions
                    .Where(r => r.Name.StartsWith(track.Name, StringComparison.OrdinalIgnoreCase)) // Example condition
                    .ToList();

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