using System.Collections.Generic;
using global::PtInfo.Core.Model;
using global::PtInfo.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace PtInfo.Core.Parsers
{


    namespace Ptformat.Core.Parsers
    {
        public class MidiEventsParser(ILogger<MidiEventsParser> logger) : IListParser<MidiEvent>
        {
            public List<MidiEvent> Parse(Queue<Block> blocks, byte[] rawFile, bool isBigEndian)
            {
                var midiEvents = new List<MidiEvent>();

                while (blocks.Count > 0)
                {
                    var block = blocks.Peek();

                    if (block.ContentType == ContentType.MidiEventsBlock)
                    {
                        blocks.Dequeue(); // Consume the block after processing it

                        var pos = (int)block.Offset + 11; // Start position after some header bytes

                        // Read number of midi events
                        var nMidiEvents = EndianReader.ReadInt32(rawFile, pos, isBigEndian);
                        pos += 4;

                        // Read the base "zero ticks" time
                        var zeroTicks = EndianReader.ReadInt64(rawFile, pos, isBigEndian);
                        pos += 8;

                        for (var i = 0; i < nMidiEvents && pos < rawFile.Length; i++)
                        {
                            var midiPos = EndianReader.ReadInt64(rawFile, pos, isBigEndian) - zeroTicks;
                            pos += 8;

                            var midiNote = rawFile[pos];
                            pos += 1;

                            var midiLength = EndianReader.ReadInt64(rawFile, pos, isBigEndian);
                            pos += 8;

                            var midiVelocity = rawFile[pos];
                            pos += 1;

                            var midiEvent = new MidiEvent
                            {
                                Position = midiPos,
                                Length = midiLength,
                                Note = midiNote,
                                Velocity = midiVelocity
                            };

                            midiEvents.Add(midiEvent);
                        }

                        logger.LogInformation("Parsed {count} MIDI events from block.", midiEvents.Count);
                    }
                    else
                    {
                        break; // Exit the loop if no more MIDI event blocks are found
                    }
                }

                return midiEvents;
            }
        }
    }

}
