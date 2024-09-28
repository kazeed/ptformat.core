using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using Ptformat.Core.Readers;

namespace Ptformat.Core
{
    public class PtFileService
    {
        private readonly ILogger<PtFileService> logger;
        private BinaryReader reader;

        public PtFileService(ILogger<PtFileService> logger, BinaryReader reader)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public List<Track> GetTracks()
        {
            var tracks = new List<Track>();
            try
            {
                int magicNumber = reader.ReadInt32();
                logger.LogInformation($"Magic Number: {magicNumber}");

                // Example logic: parsing track data
                int trackCount = reader.ReadInt32();
                var trackList = new List<Track>();
                for (int i = 0; i < trackCount; i++)
                {
                    var trackName = ReadPascalString();
                    var regionCount = reader.ReadInt32();

                    // Iterate through regions
                    var regions = new List<Region>();
                    for (var j = 0; j < regionCount; j++)
                    {
                        var region = new Region
                        {

                            StartPosition = reader.ReadInt32(),
                            EndPosition = reader.ReadInt32(),
                            Name = ReadPascalString()
                        };
                        regions.Add(region);
                    }

                    tracks.Add(new Track(trackName, i, regions));
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Reached the end of the file unexpectedly.");
            }

            return tracks;
        }

        // Helper method to read Pascal-style strings
        private string ReadPascalString()
        {
            byte length = reader.ReadByte();
            byte[] strBytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(strBytes);
        }

        public static ContentType GetContentDescription(ushort ctype) => ctype switch
        {
            0x0030 => ContentType.InfoProductVersion,
            0x1001 => ContentType.WavSampleRateSize,
            0x1003 => ContentType.WavMetadata,
            0x1004 => ContentType.WavListFull,
            0x1007 => ContentType.RegionNameNumber,
            0x1008 => ContentType.AudioRegionNameNumberV5,
            0x100b => ContentType.AudioRegionListV5,
            0x100f => ContentType.AudioRegionTrackEntry,
            0x1011 => ContentType.AudioRegionTrackMapEntries,
            0x1012 => ContentType.AudioRegionTrackFullMap,
            0x1014 => ContentType.AudioTrackNameNumber,
            0x1015 => ContentType.AudioTracks,
            0x1017 => ContentType.PluginEntry,
            0x1018 => ContentType.PluginFullList,
            0x1021 => ContentType.IOChannelEntry,
            0x1022 => ContentType.IOChannelList,
            0x1028 => ContentType.InfoSampleRate,
            0x103a => ContentType.WavNames,
            0x104f => ContentType.AudioRegionTrackSubentryV8,
            0x1050 => ContentType.AudioRegionTrackEntryV8,
            0x1052 => ContentType.AudioRegionTrackMapEntriesV8,
            0x1054 => ContentType.AudioRegionTrackFullMapV8,
            0x1056 => ContentType.MidiRegionTrackEntry,
            0x1057 => ContentType.MidiRegionTrackMapEntries,
            0x1058 => ContentType.MidiRegionTrackFullMap,
            0x2000 => ContentType.MidiEventsBlock,
            0x2001 => ContentType.MidiRegionNameNumberV5,
            0x2002 => ContentType.MidiRegionsMapV5,
            0x2067 => ContentType.InfoPathOfSession,
            0x2511 => ContentType.SnapsBlock,
            0x2519 => ContentType.MidiTrackFullList,
            0x251a => ContentType.MidiTrackNameNumber,
            0x2523 => ContentType.CompoundRegionElement,
            0x2602 => ContentType.IORoute,
            0x2603 => ContentType.IORoutingTable,
            0x2628 => ContentType.CompoundRegionGroup,
            0x2629 => ContentType.AudioRegionNameNumberV10,
            0x262a => ContentType.AudioRegionListV10,
            0x262c => ContentType.CompoundRegionFullMap,
            0x2633 => ContentType.MidiRegionsNameNumberV10,
            0x2634 => ContentType.MidiRegionsMapV10,
            0x271a => ContentType.MarkerList,
            _ => ContentType.UnknownContentType
        };
    }

}

