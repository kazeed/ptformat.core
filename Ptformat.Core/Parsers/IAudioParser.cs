using Ptformat.Core.Model;
using System.Collections.Generic;

namespace Ptformat.Core.Parsers
{
    public interface IAudioParser
    {
        List<AudioRef> ParseAudio(Queue<Block> blocks, byte[] rawFile, bool isBigEndian);
    }
}