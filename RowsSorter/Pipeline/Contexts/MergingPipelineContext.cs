using System.Buffers;
using RowsSorter.Interfaces;

namespace RowsSorter.Pipeline.Contexts;

public class MergingPipelineContext : IDisposable
{
    public IStreamCollection Readers { get; }
    public ISortingQueue SortingQueue { get; }
    public Stream Writer { get; }
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

        ArgumentOutOfRangeException.ThrowIfNotEqual<bool>(writer.CanWrite,true);
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
