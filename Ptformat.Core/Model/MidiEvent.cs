// Definition for a basic MidiEvent class (can be extended with more properties)
namespace Ptformat.Core.Model
{
    public class MidiEvent
    {
        public long Position { get; internal set; }
        public long Length { get; internal set; }
        public byte Note { get; internal set; }
        public byte Velocity { get; internal set; }
    }
}