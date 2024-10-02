using Ptformat.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace Ptformat.Core.Parsers
{
    public class MidiParser : IPtParser<MidiTrack>
    {
        public List<MidiTrack> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
        {
            var midiTracks = new List<MidiTrack>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek(); // Peek to inspect the block without removing it
                if (block.ContentType == ContentType.MidiTrackFullList)
                {
                    blocks.Dequeue(); // Dequeue the block after processing it

                    foreach (var child in block.Children.Where(c => c.ContentType == ContentType.MidiTrackNameNumber))
                    {
                        var pos = (int)child.Offset + 4;
                        var trackName = ParserUtils.ParseString(rawFile, ref pos, isBigEndian);

                        // Parse the MIDI events within the block
                        var midiEvents = ParseMidiEvents(blocks, rawFile);

                        // Create MidiTrack with the extracted track name and events
                        var midiTrack = new MidiTrack(trackName) { MidiEvents = midiEvents };

                        midiTracks.Add(midiTrack);
                    }
                }
                else
                {
                    break; // Exit if the block isn't relevant to MIDI parsing
                }
            }

            return midiTracks;
        }

        /// <summary>
        /// Parses MIDI events from the blocks.
        /// </summary>
        private static List<MidiEvent> ParseMidiEvents(Queue<Block> blocks, byte[] rawFile)
        {
            var midiEvents = new List<MidiEvent>();

            while (blocks.Count > 0)
            {
                var block = blocks.Peek();

                if (block.ContentType == ContentType.MidiEventsBlock)
                {
                    blocks.Dequeue(); // Remove block from the queue after processing it

                    // Process the MIDI events within the block
                    // Note: This example assumes the MIDI events are sequential and can be read as bytes.
                    var pos = (int)block.Offset + 2; // Example offset adjustment
                    while (pos < block.Offset + block.Size)
                    {
                        var eventType = rawFile[pos++]; // Read event type
                        var midiData1 = rawFile[pos++];  // Read MIDI data
                        var midiData2 = rawFile[pos++];  // Read MIDI data (optional, depending on event type)

                        var midiEvent = new MidiEvent(pos, eventType, midiData1, midiData2);
                        midiEvents.Add(midiEvent);
                    }
                }
                else
                {
                    break; // Exit if the block isn't relevant to MIDI events
                }
            }

            return midiEvents;
        }
    }
}