namespace PtFormat.Core.Models;

public abstract record Track<TRegion>(
    string Name,
    IReadOnlyList<TRegion> Regions
);