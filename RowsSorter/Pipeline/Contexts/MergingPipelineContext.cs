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
        Readers = readers;
        SortingQueue = sortingQueue;
        Writer = writer;
        Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    }

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
