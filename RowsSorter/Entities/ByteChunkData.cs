using RowsSorter.Extensions;
using RowsSorter.Shared;

namespace RowsSorter.Entities;

public struct ByteChunkData 
{
    internal readonly ReadOnlyMemory<byte> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteChunkData"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    public ByteChunkData(ReadOnlyMemory<byte> data)
    {
        _data = data;
    }

    /// <summary>
    /// Gets the length.
    /// </summary>
    public int Length => _data.Length;

    /// <summary>
    /// Writes the to.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void WriteTo(Stream stream)
    {
        stream.Write(_data.Span);
    }

    /// <summary>
    /// Copies the to.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="bufferOffset">The buffer offset.</param>
    public void CopyTo(byte[] buffer, int bufferOffset)
    {
        _data.Span.CopyTo(buffer.AsSpan(bufferOffset));
    }
}


