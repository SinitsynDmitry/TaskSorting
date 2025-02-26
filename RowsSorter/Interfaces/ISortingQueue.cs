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
    /// <param name="taskItem">The task item.</param>
    void Enqueue(TaskItem taskItem);
}