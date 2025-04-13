namespace PtFormat.Core.Models
{
    public abstract record Region<TSource>(
        string Name,
        long Offset,
        long Length,
        TSource Source
    );
}