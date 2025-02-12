using System.Buffers;
using RowsSorter.Entities;
using RowsSorter.Extensions;
using RowsSorter.Interfaces;

namespace RowsSorter.Merger;

internal class StreamCollection : IStreamCollection
{
    private readonly FileStream[] _readers;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamCollection"/> class.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="fileStreamProvider">The file stream provider.</param>
    /// <param name="isAsync">If true, is async.</param>
    public StreamCollection(IReadOnlyList<string> chunkFiles, IFileStreamProvider fileStreamProvider, bool isAsync = false)
    {
        _readers = chunkFiles.Select(p => fileStreamProvider.GetReadStream(p, isAsync: isAsync)).ToArray();
    }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A FileStream.</returns>
    private FileStream GetStream(int index)
    {
        return _readers[index];
    }

    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    /// <returns>A ReadOnlyMemory&lt;byte&gt;? .</returns>
    public ByteChunkData? ReadLine(int i, Span<byte> rowBuffer)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StreamCollection));

        var reader = GetStream(i);

        if (reader.Position < reader.Length)
        {
            var line = reader.ReadLine(rowBuffer);
            return line.HasValue ? new ByteChunkData(line.Value) : null;
        }
        return null;
    }


    /// <summary>
    /// Reads the line async.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask<ByteChunkData?> ReadLineAsync(int i, Memory<byte> rowBuffer)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StreamCollection));

        var reader = GetStream(i);
        if (reader.Position < reader.Length)
        {
            var line = await reader.ReadLineAsync(rowBuffer);
            return line.HasValue ? new ByteChunkData(line.Value) : null;
        }
        return null;
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    public int Count => _readers.Length;

    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var reader in _readers)
        {
            reader?.Dispose();
        }

        _disposed = true;
    }
}