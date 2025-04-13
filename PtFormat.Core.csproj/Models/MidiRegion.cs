namespace PtFormat.Core.Models;

public sealed record MidiRegion(
    string Name,
    long Offset,
    long Length,
    MidiSource Source,
    IReadOnlyList<MidiNote> Notes
) : Region<MidiSource>(Name, Offset, Length, Source);

