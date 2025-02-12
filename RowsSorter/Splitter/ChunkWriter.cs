using RowsSorter.Interfaces;
using RowsSorter.Extensions;
using RowsSorter.Shared;
using RowsSorter.Entities;

namespace RowsSorter.Splitter;
internal class ChunkWriter : IChunkWriter
{
    private readonly IFileStreamProvider _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkWriter"/> class.
    /// </summary>
    public ChunkWriter()
    {
        _streamProvider = new FileStreamProvider();
    }


    /// <summary>
    /// Writes the chunk.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    public void WriteChunk(string outputFile, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null)
    {
        using var fileStream = _streamProvider.GetWriteStream(outputFile, chunkSize);

        fileStream.WriteChunk(lines, chunkSize, buffer);
    }


    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask WriteChunkAsync(string outputFile, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null)
    {
        using var fileStream = _streamProvider.GetWriteStream(outputFile, chunkSize, true);

        await fileStream.WriteChunkAsync(lines, chunkSize, buffer);
    }
}
