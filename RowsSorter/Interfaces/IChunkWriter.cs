using RowsSorter.Entities;

namespace RowsSorter.Interfaces;

public interface IChunkWriter
{

    /// <summary>
    /// Writes the chunk.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    void WriteChunk(string outputFile, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null);

    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask WriteChunkAsync(string outputFile, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null);
}
