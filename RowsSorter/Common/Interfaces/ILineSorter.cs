using RowsSorter.Entities;
using RowsSorter.MainPipelineSteps.ChunkProcessing.Context;

namespace RowsSorter.Common.Interfaces;

public interface ILineSorter
{
    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="buffer">The _buffer.</param>
    /// <param name="totalSize">The total size.</param>
    void Sort(Memory<byte> buffer, int totalSize);

    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="context">The context.</param>
    void Sort(ChunkProcessingContext context);
}
