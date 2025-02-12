using System.Buffers;

namespace RowsSorter.Shared;

internal class PooledFileStream : FileStream
{
    private readonly byte[] _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledFileStream"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The buffer.</param>
    public PooledFileStream(FileStream stream, byte[] buffer)
        : base(stream.SafeFileHandle, FileAccess.Read, buffer.Length, stream.IsAsync)
    {
        _buffer = buffer;
    }

    /// <summary>
    /// Gets the buffer.
    /// </summary>
    public byte[] GetBuffer
    {
        get { return _buffer; }
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    /// <param name="disposing">If true, disposing.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
        base.Dispose(disposing);
    }
}
