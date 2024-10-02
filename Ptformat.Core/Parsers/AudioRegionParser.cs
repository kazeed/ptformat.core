using Microsoft.Extensions.Logging;
using PtInfo.Core.Model;
using System.Collections.Generic;

namespace PtInfo.Core.Parsers
{
    public class AudioRegionParser(ILogger<AudioRegionParser> logger) : IListParser<Region>
    {
        public List<Region> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var regions = new List<Region>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek at the block

                if (block.ContentType == ContentType.RegionNameNumber ||
                    block.ContentType == ContentType.AudioRegionNameNumberV5 ||
                    block.ContentType == ContentType.AudioRegionNameNumberV10)
                {
                    blocks.Dequeue(); // Process the block by removing it

                    var pos = (int)block.Offset + 2;
                    var regionName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);

                    var regionMetadata = ParserUtils.ParseRegionMetadata(rawFile, ref pos, isBigEndian);
                    var region = new AudioRegion
                    {
                        Name = regionName,
                        Start = regionMetadata.Start,
                        Offset = regionMetadata.Offset,
                        Length = regionMetadata.Length
                    };

                    logger.LogInformation("Parsed region: {regionName}, Start: {start}, Length: {length}", regionName, region.Start, region.Length);
                    regions.Add(region);
                }
                else
                {
                    break; // If the block isn't related to regions, stop processing
                }
            }

            return regions;
        }
    }
}