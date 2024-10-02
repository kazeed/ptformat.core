using Microsoft.Extensions.Logging;
using PtInfo.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace PtInfo.Core.Parsers
{
    public class MidiRegionParser(ILogger<MidiRegionParser> logger) : IListParser<Region>
    {
        private readonly ILogger<MidiRegionParser> logger = logger;

        public List<Region> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var midiRegions = new List<Region>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek();

                if (block.ContentType == ContentType.MidiRegionsMapV5 ||
                    block.ContentType == ContentType.MidiRegionsMapV10)
                {
                    blocks.Dequeue();

                    foreach (var child in block.Children.Where(c => c.ContentType == ContentType.MidiRegionNameNumberV5 ||
                                                                    c.ContentType == ContentType.MidiRegionsNameNumberV10))
                    {
                        var pos = (int)child.Offset + 2;
                        var regionName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);

                        var regionMetadata = ParserUtils.ParseRegionMetadata(rawFile, ref pos, isBigEndian);
                        var midiRegion = new MidiRegion
                        {
                            Name = regionName,
                            Start = regionMetadata.Start,
                            Offset = regionMetadata.Offset,
                            Length = regionMetadata.Length
                        };

                        logger.LogInformation("Parsed MIDI region: {regionName}, Start: {start}, Length: {length}", regionName, midiRegion.Start, midiRegion.Length);
                        midiRegions.Add(midiRegion);
                    }
                }
                else
                {
                    break; // Stop processing if the block isn't related to MIDI regions
                }
            }

            return midiRegions;
        }
    }

}
