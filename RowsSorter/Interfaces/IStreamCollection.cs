using RowsSorter.Entities;

namespace RowsSorter.Interfaces;

public interface IStreamCollection : IDisposable
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <param name="rowBuffer">The row _buffer.</param>
    /// <returns>A ReadOnlyMemory&lt;byte&gt;? .</returns>
    ByteChunkData? ReadLine(int i, Span<byte> rowBuffer);

    /// <summary>
    /// Reads the line async.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <param name="rowBuffer">The row _buffer.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask<ByteChunkData?> ReadLineAsync(int i, Memory<byte> rowBuffer);
}