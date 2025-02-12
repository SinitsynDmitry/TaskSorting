using RowsSorter.Entities;
using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Merger;

internal class StreamMergeQueue : ISortingQueue
{
    private readonly PriorityQueue<TaskItem, ByteChunkData> _priorityQueue;

    //public StreamMergeQueue()
    //{
    //    _dataComparer = new ReadOnlyMemoryByteComparer();
    //    _priorityQueue = new PriorityQueue<TaskItem, ReadOnlyMemory<byte>>(_dataComparer);
    //}

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMergeQueue"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public StreamMergeQueue(IComparer<ByteChunkData> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        _priorityQueue = new PriorityQueue<TaskItem, ByteChunkData>(comparer);

    }

    /// <summary>
    /// Enqueues the.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="index">The index.</param>
    public void Enqueue(ByteChunkData line, int index)
    {
        _priorityQueue.Enqueue(new TaskItem(line, index), line);
    }

    /// <summary>
    /// Dequeues the.
    /// </summary>
    /// <returns>A TaskItem.</returns>
    public TaskItem Dequeue()
    {
        return _priorityQueue.Dequeue();
    }

    /// <summary>
    /// Clears the.
    /// </summary>
    public void Clear()
    {
        _priorityQueue.Clear();
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    public int Count
    {
        get
        {
            return _priorityQueue.Count;
        }
    }

}