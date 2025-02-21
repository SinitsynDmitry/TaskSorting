using RowsSorter.Entities;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Interfaces;

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
