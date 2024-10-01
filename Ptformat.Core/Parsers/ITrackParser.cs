using Ptformat.Core.Model;
using System.Collections.Generic;

namespace Ptformat.Core.Parsers
{
    public interface ITrackParser
    {
        List<Track> ParseTracks(Queue<Block> blocks, byte[] rawFile, bool isBigEndian);
    }
}