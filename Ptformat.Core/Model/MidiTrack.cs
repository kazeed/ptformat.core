﻿using System.Collections.Generic;

namespace PtInfo.Core.Model
{
    public class MidiTrack(string name) : Track(name)
    {
        // List of MIDI events or data associated with the MIDI track
        public List<MidiEvent> MidiEvents { get; set; } = [];
        public int Index { get; internal set; }

        // Method to add a MIDI event to the track
        public void AddMidiEvent(MidiEvent midiEvent)
        {
            MidiEvents.Add(midiEvent);
        }
    }
}