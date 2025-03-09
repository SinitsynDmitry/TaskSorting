using System.Buffers;
using RowsSorter.Common.Interfaces;

namespace RowsSorter.MainPipelineSteps.ChunkMerge.Context;

public class MergingPipelineContext : IDisposable
{
    /// <summary>
    /// Gets the readers.
    /// </summary>
    public IStreamCollection Readers { get; }
    /// <summary>
    /// Gets the sorting queue.
    /// </summary>
    public ISortingQueue SortingQueue { get; }
    /// <summary>
    /// Gets the writer.
    /// </summary>
    public Stream Writer { get; }
    /// <summary>
    /// Gets the buffer.
    /// </summary>
    public byte[] Buffer { get; private set; }

    public MergingPipelineContext(
        IStreamCollection readers,
        ISortingQueue sortingQueue,
        Stream writer,
        int bufferSize = 128)
    {
        ArgumentNullException.ThrowIfNull(readers);
        ArgumentNullException.ThrowIfNull(sortingQueue);
        ArgumentNullException.ThrowIfNull(writer);

        ArgumentOutOfRangeException.ThrowIfNotEqual(writer.CanWrite,true);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(bufferSize, 0);

        Readers = readers;
        SortingQueue = sortingQueue;
        Writer = writer;
        Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        if (Buffer != null)
        {
            ArrayPool<byte>.Shared.Return(Buffer, true);
            Buffer = null;
        }

        SortingQueue.Clear();
        Readers.Dispose();
        Writer.Dispose();
    }
}
