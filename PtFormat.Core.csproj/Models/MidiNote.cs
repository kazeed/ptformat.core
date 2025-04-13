namespace PtFormat.Core.Models;

public sealed record MidiNote(
    long Offset,
    long Length,
    byte Pitch,
    byte Velocity
);
