// Definition for a basic MidiEvent class (can be extended with more properties)
namespace Ptformat.Core.Model
{
    public class MidiEvent
    {
        public long Start { get; set; }
        public long Duration { get; set; }
        public byte MidiData1 { get; set; } // This can represent the MIDI event type (e.g., Note On, Note Off)

        public byte MidiData2 { get; set; } // This can represent the MIDI event type (e.g., Note On, Note Off)

        public MidiEvent(long start, long duration, byte midiData1)
        {
            Start = start;
            Duration = duration;
            MidiData1 = midiData1;
        }

        public MidiEvent(long start, long duration, byte midiData1, byte midiData2)
        {
            Start = start;
            Duration = duration;
            MidiData1 = midiData1;
            MidiData2 = midiData2;
        }
    }
}