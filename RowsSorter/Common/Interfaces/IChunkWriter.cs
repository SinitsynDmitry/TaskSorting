using RowsSorter.MainPipelineSteps.ChunkProcessing.Context;

namespace RowsSorter.Common.Interfaces;

public interface IChunkWriter
{

    /// <summary>
    /// Writes the chunk.
    /// </summary>
    /// <param name="context">The context.</param>
    void WriteChunk(ChunkProcessingContext context);

    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask WriteChunkAsync(ChunkProcessingContext context);
}
