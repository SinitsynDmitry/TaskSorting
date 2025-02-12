using System.Buffers;
using RowsSorter.Entities;
using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Merger;

internal class ChunkMerger : IChunkMerger
{
    private readonly IComparer<ByteChunkData> _comparer;
    private readonly IFileStreamProvider _streamProvider;
    const int MEMORY_BUFFER_SIZE = 65536;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkMerger"/> class.
    /// </summary>
    public ChunkMerger()
    {
        _comparer = new ChunkDataComparer();
        _streamProvider = new FileStreamProvider();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkMerger"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public ChunkMerger(IComparer<ByteChunkData> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        _comparer = comparer;
        _streamProvider = new FileStreamProvider();
    }

    /// <summary>
    /// Merges the sorted chunks.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    public void MergeSortedChunks(IReadOnlyList<string> chunkFiles, string outputFile)
    {
        using (var readers = new StreamCollection(chunkFiles, _streamProvider))
        using (var writer = _streamProvider.GetWriteStream(outputFile))
        using (var bufferedWriter = new BufferedStream(writer, MEMORY_BUFFER_SIZE))
        {
            MergeStreams(readers, bufferedWriter);

            bufferedWriter.Flush();
        }
        _streamProvider.DeleteFiles(chunkFiles);
    }

    /// <summary>
    /// Merges the streams.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="writer">The writer.</param>
    private void MergeStreams(IStreamCollection readers, Stream writer)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var rowBuffer = arrayPool.Rent(512);
        var sortingQueue = new StreamMergeQueue(_comparer);

        try
        {
            InitializeQueue(readers, sortingQueue, rowBuffer);
            ProcessQueue(readers, sortingQueue, writer, rowBuffer);
            sortingQueue.Clear();
        }
        finally
        {
            arrayPool.Return(rowBuffer);
        }
    }

    /// <summary>
    /// Merges the streams async.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="writer">The writer.</param>
    /// <returns>A ValueTask.</returns>
    private async ValueTask MergeStreamsAsync(IStreamCollection readers, Stream writer)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var rowBuffer = arrayPool.Rent(512);
        var sortingQueue = new StreamMergeQueue(_comparer);

        try
        {
            await InitializeQueueAsync(readers, sortingQueue, rowBuffer);
            await ProcessQueueAsync(readers, sortingQueue, writer, rowBuffer);
            sortingQueue.Clear();
        }
        finally
        {
            arrayPool.Return(rowBuffer);
        }
    }

    /// <summary>
    /// Initializes the queue.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="sortingQueue">The sorting queue.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    private void InitializeQueue(IStreamCollection readers, ISortingQueue sortingQueue, Span<byte> rowBuffer)
    {
        for (int i = 0; i < readers.Count; i++)
        {
            var line = readers.ReadLine(i, rowBuffer);
            if (line is not null)
            {
                sortingQueue.Enqueue(line.Value, i);
            }
        }
    }

    /// <summary>
    /// Processes the queue.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="sortingQueue">The sorting queue.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    private void ProcessQueue(IStreamCollection readers, ISortingQueue sortingQueue, Stream writer, Span<byte> rowBuffer)
    {
        while (sortingQueue.Count > 0)
        {
            var taskItem = sortingQueue.Dequeue();
            taskItem.Value.WriteTo(writer);
            var index = taskItem.Index;

            var line = readers.ReadLine(index, rowBuffer);

            if (line is not null)
            {
                sortingQueue.Enqueue(line.Value, index);
            }
        }
    }


    /// <summary>
    /// Merges the sorted chunks async.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    /// <returns>A Task.</returns>
    public async ValueTask MergeSortedChunksAsync(IReadOnlyList<string> chunkFiles, string outputFile)
    {
        using (var readers = new StreamCollection(chunkFiles, _streamProvider, true))
        using (var writer = _streamProvider.GetWriteStream(outputFile, isAsync: true))
        using (var bufferedWriter = new BufferedStream(writer, MEMORY_BUFFER_SIZE))
        {
            await MergeStreamsAsync(readers, bufferedWriter);

            await bufferedWriter.FlushAsync();
        }
        _streamProvider.DeleteFiles(chunkFiles);
    }

    /// <summary>
    /// Initializes the queue.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="sortingQueue">The sorting queue.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    private async ValueTask InitializeQueueAsync(IStreamCollection readers, ISortingQueue sortingQueue, Memory<byte> rowBuffer)
    {
        for (int i = 0; i < readers.Count; i++)
        {
            var line = await readers.ReadLineAsync(i, rowBuffer);

            if (line is not null)
            {
                sortingQueue.Enqueue(line.Value, i);
            }
        }
    }

    /// <summary>
    /// Processes the queue.
    /// </summary>
    /// <param name="readers">The readers.</param>
    /// <param name="sortingQueue">The sorting queue.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="rowBuffer">The row buffer.</param>
    private async ValueTask ProcessQueueAsync(IStreamCollection readers, ISortingQueue sortingQueue, Stream writer, Memory<byte> rowBuffer)
    {
        while (sortingQueue.Count > 0)
        {
            var taskItem = sortingQueue.Dequeue();
            taskItem.Value.WriteTo(writer);

            var index = taskItem.Index;

            var line = await readers.ReadLineAsync(index, rowBuffer);

            if (line is not null)
            {
                sortingQueue.Enqueue(line.Value, index);
            }
        }
    }

}
