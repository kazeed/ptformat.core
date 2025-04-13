namespace PtFormat.Core.Models;

public sealed record MidiTrack(
    string Name,
    IReadOnlyList<MidiRegion> Regions
) : Track<MidiRegion>(Name, Regions);
