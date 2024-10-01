using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ptformat.Core.Parsers
{
    public class TrackParser(ILogger<TrackParser> logger) : ITrackParser
    {
        private readonly ILogger<TrackParser> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Parses the blocks to extract track information.
        /// </summary>
        public List<Track> ParseTracks(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var tracks = new List<Track>();

            while (blocks.TryDequeue(out var block))
            {
                if (block.ContentType == ContentType.AudioTracks)
                {
                    logger.LogInformation("Parsing AudioTracks block at offset {Offset}", block.Offset);

                    foreach (var child in block.Children.Where(c => c.ContentType == ContentType.AudioTrackNameNumber))
                    {
                        var pos = (int)child.Offset + 2;
                        var trackName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);

                        pos += 5; // Skipping additional fields
                        var numberOfChannels = EndianReader.ReadInt32(rawFile, pos, isBigEndian);
                        pos += 4;

                        var channels = new List<int>();

                        for (var i = 0; i < numberOfChannels; i++)
                        {
                            var channel = EndianReader.ReadInt16(rawFile, pos, isBigEndian);
                            channels.Add(channel);
                            pos += 2;
                        }

                        var track = new Track
                        {
                            Name = trackName,
                            Channels = channels,
                        };

                        logger.LogInformation("Parsed Track: {TrackName} with {ChannelCount} channels", trackName, numberOfChannels);
                        tracks.Add(track);
                    }
                }
                else
                {
                    // If it's not a track-related block, put it back into the queue for other parsers to handle
                    blocks.Enqueue(block);
                }
            }

            return tracks;
        }
    }
}
