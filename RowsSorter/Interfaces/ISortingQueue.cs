using RowsSorter.Entities;

namespace RowsSorter.Interfaces;

public interface ISortingQueue
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Clears the.
    /// </summary>
    void Clear();

    /// <summary>
    /// Dequeues the.
    /// </summary>
    /// <returns>A TaskItem.</returns>
    TaskItem Dequeue();

    /// <summary>
    /// Enqueues the.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="index">The index.</param>
    void Enqueue(ByteChunkData line, int index);
}