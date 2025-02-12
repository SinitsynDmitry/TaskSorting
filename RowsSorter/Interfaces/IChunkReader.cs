using RowsSorter.Entities;

namespace RowsSorter.Interfaces;

public interface IChunkReader
{

	/// <summary>
	/// Reads the chunk.
	/// </summary>
	/// <param name="inputFile">The input file.</param>
	/// <param name="startPosition">The start position.</param>
	/// <param name="chunkSize">The chunk size.</param>
	/// <param name="buffer">The buffer.</param>
	/// <returns>A ReadOnlyMemory.</returns>
	ReadOnlyMemory<byte> ReadChunk(string inputFile, long startPosition, int chunkSize, byte[]? buffer = null);

    /// <summary>
    /// Reads the chunk async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A Task.</returns>
    ValueTask<ReadOnlyMemory<byte>> ReadChunkAsync(string inputFile, long startPosition, int chunkSize, byte[]? buffer = null);

}
