using System;

namespace Ptformat.Core.Model
{
    public class AudioRef
    {
        public AudioRef(int index, string filename)
        {
            Index = index;
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public AudioRef(long length, int index, string filename)
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