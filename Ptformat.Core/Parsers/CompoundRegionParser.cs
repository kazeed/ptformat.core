using Microsoft.Extensions.Logging;
using Ptformat.Core.Model;
using System.Collections.Generic;
using System;

namespace Ptformat.Core.Parsers
{
    public class CompoundRegionParser(ILogger<CompoundRegionParser> logger) : IListParser<Region>
    {
        private readonly ILogger<CompoundRegionParser> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public List<Region> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var compoundRegions = new List<Region>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek without dequeuing

                if (block.ContentType == ContentType.CompoundRegionFullMap)
                {
                    blocks.Dequeue(); // Consume this block
                    ParseCompoundRegions(block, rawFile, isBigEndian, compoundRegions);
                }
                else
                {
                    break; // Exit when no more relevant blocks are found
                }
            }

            return compoundRegions;
        }

        private void ParseCompoundRegions(Block block, byte[] rawFile, bool isBigEndian, List<Region> compoundRegions)
        {
            foreach (var child in block.Children)
            {
                if (child.ContentType == ContentType.CompoundRegionElement)
                {
                    var pos = (int)child.Offset + 2; // Starting position for parsing
                    var regionName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);

                    var metadata = ParserUtils.ParseRegionMetadata(rawFile, ref pos, isBigEndian);
                    var compoundRegion = new MidiRegion
                    {
                        Name = regionName,
                        Start = metadata.Start,
                        Offset = metadata.Offset,
                        Length = metadata.Length
                    };

                    logger.LogInformation("Parsed compound region: {regionName}, Start: {start}, Offset: {offset}, Length: {length}",
                        regionName, compoundRegion.Start, compoundRegion.Offset, compoundRegion.Length);

                    compoundRegions.Add(compoundRegion);
                }
            }
        }
    }
}