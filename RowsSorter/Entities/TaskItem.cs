namespace RowsSorter.Entities;

public class TaskItem
{

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public ByteChunkData Value { get; set; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskItem"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    public TaskItem(ByteChunkData value, int index)
    {
        Value = value;
        Index = index;
    }
}

