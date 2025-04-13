namespace PtFormat.Core.Models;

public sealed record AudioTrack(
    string Name,
    IReadOnlyList<AudioRegion> Regions
) : Track<AudioRegion>(Name, Regions);
