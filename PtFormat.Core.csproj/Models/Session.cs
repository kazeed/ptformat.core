namespace PtFormat.Core.Models;

public sealed record Session(
    string Name,
    int SampleRate,
    IReadOnlyList<AudioSource> AudioSources,
    IReadOnlyList<AudioTrack> AudioTracks,
    IReadOnlyList<MidiSource> MidiSources,
    IReadOnlyList<MidiTrack> MidiTracks
);
