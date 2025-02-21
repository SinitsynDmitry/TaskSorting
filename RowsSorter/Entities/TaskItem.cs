namespace RowsSorter.Entities;

public struct TaskItem
{
    /// <summary>
    /// Gets the row.
    /// </summary>
    public ByteChunkData Value { get; init; }

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskItem"/> class.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="priority">The priority.</param>
    public TaskItem(ByteChunkData row, int priority)
    {
        Value = row;
        Index = priority;
    }
}

