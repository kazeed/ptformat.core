using Microsoft.Extensions.Logging;
using PtInfo.Core.Model;
using PtInfo.Core.Utilities;
using System.Collections.Generic;

namespace PtInfo.Core.Parsers
{
    public class HeaderParser(ILogger<HeaderParser> logger) : ISingleParser<HeaderInfo>
    {
        public HeaderInfo Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            string sessionName = string.Empty;
            int sampleRate = 0;
            string productVersion = string.Empty;

            while (blocks.Count > 0)
            {
                var block = blocks.Dequeue();

                switch (block.ContentType)
                {
                    case ContentType.InfoProductVersion:
                        // Extract product version (Field 2)
                        var posProductVersion = (int)block.Offset + 2;
                        productVersion = ParserUtils.ParseString(rawFile, ref posProductVersion, isBigEndian);
                        logger.LogInformation("Product Version: {productVersion}", productVersion);
                        break;

                    case ContentType.InfoSampleRate:
                        // Extract sample rate (Field in its own block)
                        sampleRate = EndianReader.ReadInt32(rawFile, (int)block.Offset + 4, isBigEndian);
                        logger.LogInformation("Sample Rate: {sampleRate}", sampleRate);
                        break;

                    default:
                        // Check for the session name (Field 5: .pts string)
                        var posSessionName = (int)block.Offset + 2;
                        var potentialSessionName = ParserUtils.ParseString(rawFile, ref posSessionName, isBigEndian);
                        if (potentialSessionName.EndsWith(".pts"))
                        {
                            sessionName = potentialSessionName;
                            logger.LogInformation("Session Name: {sessionName}", sessionName);
                        }
                        break;
                }

                // Exit early if all necessary fields are found
                if (!string.IsNullOrEmpty(sessionName) && sampleRate > 0 && !string.IsNullOrEmpty(productVersion))
                {
                    break;
                }
            }

            return new HeaderInfo
            {
                Name = sessionName,
                SampleRate = sampleRate,
                ProductVersion = productVersion
            };
        }
    }
}