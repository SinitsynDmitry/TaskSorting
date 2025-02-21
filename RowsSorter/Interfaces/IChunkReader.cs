using RowsSorter.Entities;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Interfaces;

public interface IChunkReader
{

    /// <summary>
    /// Reads the chunk.
    /// </summary>
    /// <param name="context">The context.</param>
    void ReadChunk(ChunkProcessingContext context);

    /// <summary>
    /// Reads the chunk async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask ReadChunkAsync(ChunkProcessingContext context);

}
