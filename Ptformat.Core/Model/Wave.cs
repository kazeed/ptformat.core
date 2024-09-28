using System;

namespace Ptformat.Core.Model
{
    public class WavFile
    {
        public int Index { get; }
        public string Filename { get; set; }
        public long Length { get; set; }

        public WavFile(int index)
        {
            Index = index;
        }
    }
}

