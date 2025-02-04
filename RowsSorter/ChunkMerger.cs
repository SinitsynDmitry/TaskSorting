using RowsSorter.Interfaces;

namespace RowsSorter;

internal class ChunkMerger: IChunkMerger
{
    private readonly IComparer<ReadOnlyMemory<byte>> _comparer;
    private readonly IStreamLineReader _streamLineReader;

    const int BUFFER_SIZE = 64 * 1024;
    const int MEMORY_BUFFER_SIZE = 4096;

    public ChunkMerger()
    {
        _comparer = new ReadOnlyMemoryByteComparer();
        _streamLineReader = new StreamLineReader();
    }

    public ChunkMerger(IComparer<ReadOnlyMemory<byte>> comparer, IStreamLineReader streamLineReader)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(streamLineReader);
        _comparer = comparer;
        _streamLineReader = streamLineReader;
    }
    /// <summary>
    /// Merges the sorted chunks.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    public void MergeSortedChunks(IReadOnlyList<string> chunkFiles, string outputFile)
    {
        var readers = chunkFiles
             .Select(file => new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan))
             .ToArray();
        var priorityQueue = new PriorityQueue<TaskItem, ReadOnlyMemory<byte>>(_comparer);

        using (var writer = new BufferedStream(new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, FileOptions.WriteThrough), BUFFER_SIZE))
        {
            using (MemoryStream rowBuffer = new MemoryStream(MEMORY_BUFFER_SIZE))
            {

                for (int i = 0; i < readers.Length; i++)
                {
                    var reader = readers[i];
                    if (reader.Position < reader.Length)
                    {
                        var endOfFile = _streamLineReader.ReadLine(reader, rowBuffer);
                        if (endOfFile) break;
                        var line = new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position);
                        priorityQueue.Enqueue(new TaskItem(line, i), line);
                    }
                }

                while (priorityQueue.Count > 0)
                {
                    var taskItem = priorityQueue.Dequeue();
                    writer.Write(taskItem.Row.Span);
                    var reader = readers[taskItem.Priority];
                    if (reader.Position < reader.Length)
                    {
                        var endOfFile = _streamLineReader.ReadLine(reader, rowBuffer);
                        if (endOfFile) break;
                        var nextLine = new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position);
                        priorityQueue.Enqueue(new TaskItem(nextLine, taskItem.Priority), nextLine);
                    }
                }
            }

            writer.Flush();
        }

        priorityQueue.Clear();
        // Cleanup

        foreach (var reader in readers)
        {
            reader.Close();
            reader.Dispose();
        }
        foreach (var oldChunk in chunkFiles)
        {
            File.Delete(oldChunk);
        }
    }

    /// <summary>
    /// Merges the sorted chunks async.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    /// <returns>A Task.</returns>
    public async Task MergeSortedChunksAsync(IReadOnlyList<string> chunkFiles, string outputFile)
    {
        var readers = chunkFiles
            .Select(file => new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.Asynchronous | FileOptions.SequentialScan))
            .ToArray();

        var priorityQueue = new PriorityQueue<TaskItem, ReadOnlyMemory<byte>>(_comparer);

        await using (var writer = new BufferedStream(new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous | FileOptions.WriteThrough), BUFFER_SIZE))
        {
            using (var rowBuffer = new MemoryStream(MEMORY_BUFFER_SIZE))
            {
                // Read initial lines from all readers and enqueue them
                for (int i = 0; i < readers.Length; i++)
                {
                    var reader = readers[i];
                    if (reader.Position < reader.Length)
                    {
                        var endOfFile = await _streamLineReader.ReadLineAsync(reader, rowBuffer);
                        if (endOfFile) break;

                        var line = new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position);
                        priorityQueue.Enqueue(new TaskItem(line, i), line);
                    }
                }

                // Merge process
                while (priorityQueue.Count > 0)
                {
                    var taskItem = priorityQueue.Dequeue();
                    await writer.WriteAsync(taskItem.Row);

                    var reader = readers[taskItem.Priority];
                    if (reader.Position < reader.Length)
                    {
                        var endOfFile = await _streamLineReader.ReadLineAsync(reader, rowBuffer);
                        if (endOfFile) break;

                        var nextLine = new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position);
                        priorityQueue.Enqueue(new TaskItem(nextLine, taskItem.Priority), nextLine);
                    }
                }
            }

            await writer.FlushAsync();
        }

        priorityQueue.Clear();

        // Cleanup
        foreach (var reader in readers)
        {
            await reader.DisposeAsync();
        }
        foreach (var oldChunk in chunkFiles)
        {
            try
            {
                await Task.Run(() => File.Delete(oldChunk)); // Ensure async delete
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete file {oldChunk}: {ex.Message}");
            }
        }
    }

}
