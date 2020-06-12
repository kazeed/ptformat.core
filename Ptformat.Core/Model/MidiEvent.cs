namespace Ptformat.Core.Model
{
    public class MidiEvent
    {
        public long Position { get; set; }

        public long Length { get; set; }

        public short Note { get; set; }

        public short Velocity { get; set; }
    }
}
