namespace PtFormat.Core.Parsing;

public sealed class UnxorStream : Stream
{
    private const int XorStartOffset = 0x400;
    private const byte XorMask = 0xA5;

    private readonly Stream innerStream;
    private long position;

    public UnxorStream(Stream baseStream)
    {
        innerStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        if (!innerStream.CanRead)
            throw new ArgumentException("Base stream must be readable.", nameof(baseStream));
    }

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => innerStream.CanSeek;
    public override bool CanWrite => false;

    public override long Length => innerStream.Length;

    public override long Position
    {
        get => position;
        set
        {
            innerStream.Position = value;
            position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = innerStream.Read(buffer, offset, count);

        ApplyXor(buffer.AsSpan(offset, bytesRead));
        position += bytesRead;
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytesRead = await innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

        ApplyXor(buffer.Span[..bytesRead]);
        position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPos = innerStream.Seek(offset, origin);
        position = newPos;
        return newPos;
    }

    public override void Flush() => innerStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        innerStream.FlushAsync(cancellationToken);

    public override void SetLength(long value) =>
        throw new NotSupportedException("UnxorStream does not support writing.");

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException("UnxorStream is read-only.");

    private void ApplyXor(Span<byte> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (position + i >= XorStartOffset)
                span[i] ^= XorMask;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            innerStream.Dispose();

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await innerStream.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
