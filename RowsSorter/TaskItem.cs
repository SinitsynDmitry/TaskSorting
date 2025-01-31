namespace RowsSorter;

internal struct TaskItem
{
    /// <summary>
    /// Gets the row.
    /// </summary>
    public ReadOnlyMemory<byte> Row { get; init; }

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskItem"/> class.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="priority">The priority.</param>
    public TaskItem(ReadOnlyMemory<byte> row, int priority)
    {
        Row = row;
        Priority = priority;
    }
}

