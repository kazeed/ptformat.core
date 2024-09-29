using System;
using System.Runtime.CompilerServices;

namespace Ptformat.Core.Model
{

    public class WavFile
    {
        public WavFile(int index, string filename)
        {
            Index = index;
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public WavFile(long length, int index, string filename)
        {
            Length = length;
            Index = index;
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public long Length { get; set; }
        public int Index { get; }
        public string Filename { get; set; }
    }
}

