namespace PtFormat.Core.Models;

public sealed record AudioSource(
    string Filename,
    string Name,
    long Length
);
