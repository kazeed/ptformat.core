namespace PtFormat.Core.Models
{
    public sealed record AudioRegion(
        string Name,
        long Offset,
        long Length,
        AudioSource Source
    ) : Region<AudioSource>(Name, Offset, Length, Source);
}