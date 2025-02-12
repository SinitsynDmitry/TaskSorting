using RowsSorter.Extensions;
using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Splitter;

internal class ChunkReader : IChunkReader
{
    private readonly IFileStreamProvider _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkReader"/> class.
    /// </summary>
    public ChunkReader()
    {
        _streamProvider = new FileStreamProvider();
    }

    /// <summary>
    /// Reads the chunk.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ReadOnlyMemory.</returns>
    public ReadOnlyMemory<byte> ReadChunk(string inputFile, long startPosition, int chunkSize, byte[]? buffer = null)
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, chunkSize);

        return fileStream.ReadChunk(startPosition, chunkSize, buffer);
    }

    /// <summary>
    /// Reads the chunk async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A Task.</returns>
    public async ValueTask<ReadOnlyMemory<byte>> ReadChunkAsync(string inputFile, long startPosition, int chunkSize, byte[]? buffer = null)
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, chunkSize, true);

        return await fileStream.ReadChunkAsync(startPosition, chunkSize, buffer);
    }
}
