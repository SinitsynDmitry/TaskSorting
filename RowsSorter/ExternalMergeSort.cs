using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RowsSorter;
public class ExternalMergeSort
{
    private readonly IComparer<ReadOnlyMemory<byte>> _comparer;
    const int BUFFER_SIZE = 64 * 1024;
    const int MEMORY_BUFFER_SIZE = 4096;
    const int MERGE_CYCLE = 30;


    public ExternalMergeSort()
    {
        _comparer = new ReadOnlyMemoryByteComparer();
    }
    public ExternalMergeSort(IComparer<ReadOnlyMemory<byte>> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    /// <summary>
    /// Sorts the large file.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    public void SortLargeFile(string inputFile, string outputFile, int linesPerFile)
    {
        var tempOutputDir = Path.Combine(Path.GetDirectoryName(outputFile)!, "temp");
        if (!Directory.Exists(tempOutputDir))
        {
            Directory.CreateDirectory(tempOutputDir);
        }

        // Step 1: Synchronously split file into sorted chunks
        var chunks = SplitFile(inputFile, linesPerFile, tempOutputDir);
        var counter = 0;
        // Step 2: Parallel merge if there are many chunks
        while (chunks.Count > MERGE_CYCLE)
        {
            int partSize = (int)Math.Sqrt(chunks.Count);
            var rangePartitioner = Partitioner.Create(0, chunks.Count, partSize);
            var chunkFilesInternal = new ConcurrentBag<string>();

            Parallel.ForEach(rangePartitioner, range =>
            {
                var chunkRange = chunks.GetRange(range.Item1, range.Item2 - range.Item1);
                var temp = Path.Combine(tempOutputDir, $"chunk_{counter}_{range.Item1}_{range.Item2}.tmp");
                MergeSortedChunks(chunkRange, temp);
                chunkFilesInternal.Add(temp);
            });

            chunks = chunkFilesInternal.ToList();
            counter++;
        }

        // Step 3: Final synchronous merge
        MergeSortedChunks(chunks, outputFile);
    }


    /// <summary>
    /// Sorts the large file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    /// <returns>A Task.</returns>
    public async Task SortLargeFileAsync(string inputFile, string outputFile, int linesPerFile)
    {
        var tempOutputDir = Path.Combine(Path.GetDirectoryName(outputFile)!, "temp");
        if (!Directory.Exists(tempOutputDir))
        {
            Directory.CreateDirectory(tempOutputDir);
        }

        // Start splitting and get chunks
        var chunks = await SplitFileAsync(inputFile, linesPerFile, tempOutputDir);

        while (chunks.Count > MERGE_CYCLE)
        {
            int partSize = (int)Math.Sqrt(chunks.Count);
            var rangePartitioner = Partitioner.Create(0, chunks.Count, partSize).AsParallel();
            var chunkFilesInternal = new ConcurrentBag<string>();
            var mergeTasks = new List<Task>();
            foreach (var range in rangePartitioner)
            {
                mergeTasks.Add(Task.Run(async () =>
                {
                    var chunkRange = chunks.GetRange(range.Item1, range.Item2 - range.Item1);
                    var temp = Path.Combine(tempOutputDir, $"chunk_{range.Item1}_{range.Item2}.tmp");
                    await MergeSortedChunksAsync(chunkRange, temp);
                    chunkFilesInternal.Add(temp);
                }));
            }

            await Task.WhenAll(mergeTasks);

            chunks = chunkFilesInternal.ToList();
        }

        // Merge the sorted chunks
        await MergeSortedChunksAsync(chunks, outputFile);
    }
    /// <summary>
    /// Split file into sorted chunks
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="linesPerFile"></param>
    /// <param name="tempOutputDir"></param>
    /// <returns></returns>
    public List<string> SplitFile(string inputFile, int linesPerFile, string tempOutputDir)
    {
        var fileInfo = new FileInfo(inputFile);
        int numThreads = Environment.ProcessorCount;
        long chunkSize = fileInfo.Length / numThreads;

        var tasks = new List<Task<List<string>>>();
        var lockObj = new object();
        var fileCounter = 0;
        var startPositions = new List<long>() { 0L };

        // Determine chunk start positions
        using (var readerX = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan))
        {
            for (int i = 1; i < numThreads; i++)
            {
                long startPosition = i * chunkSize;
                readerX.Position = startPosition;
                int currentByte;
                while ((currentByte = readerX.ReadByte()) != -1 && currentByte != '\n')
                {
                    startPosition++;
                }
                startPosition++;
                startPositions.Add(startPosition);
            }
        }

        for (int i = 0; i < numThreads; i++)
        {
            var threadIndex = i;
            var task = Task.Run(() =>
            {
                var threadOutputFiles = new List<string>();
                using var reader = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.SequentialScan);

                long startPosition = startPositions[threadIndex];
                reader.Position = startPosition;

                long endPosition = threadIndex == numThreads - 1 ? fileInfo.Length : startPositions[threadIndex + 1] - 1;

                using var rowBuffer = new MemoryStream(MEMORY_BUFFER_SIZE);

                var lineCount = 0;
                var chunkSize = 0L;
                List<ReadOnlyMemory<byte>> lines = new();

                while (reader.Position < endPosition || threadIndex == numThreads - 1)
                {
                    rowBuffer.SetLength(0);
                    rowBuffer.Position = 0;

                    var endOfFile = ReadLine(reader, rowBuffer);
                    if (endOfFile) break;
                    lines.Add(new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position));

                    chunkSize += rowBuffer.Position;
                    lineCount++;

                    if (lineCount >= linesPerFile)
                    {
                        lines.Sort(_comparer);

                        string outputFile;
                        lock (lockObj)
                        {
                            outputFile = Path.Combine(tempOutputDir, $"chunk_{fileCounter++}.tmp");
                        }

                        WriteChunks(outputFile, lines, (int)chunkSize);

                        lines.Clear();
                        threadOutputFiles.Add(outputFile);
                        chunkSize = 0L;
                        lineCount = 0;
                    }
                }

                if (lines.Count > 0)
                {
                    string outputFile;
                    lock (lockObj)
                    {
                        outputFile = Path.Combine(tempOutputDir, $"chunk_{fileCounter++}.tmp");
                    }

                    lines.Sort(_comparer);

                    WriteChunks(outputFile, lines, (int)chunkSize);

                    lines.Clear();
                    threadOutputFiles.Add(outputFile);
                }

                return threadOutputFiles;
            });

            tasks.Add(task);
        }

        return tasks.SelectMany(t => t.Result).ToList();
    }

    /// <summary>
    /// Splits the file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    /// <param name="tempOutputDir">The temp output dir.</param>
    /// <returns>A Task.</returns>
    public async Task<List<string>> SplitFileAsync(string inputFile, int linesPerFile, string tempOutputDir)
    {
        var fileInfo = new FileInfo(inputFile);
        int numThreads = Environment.ProcessorCount;
        long chunkSize = fileInfo.Length / numThreads;

        var tasks = new List<Task<List<string>>>();
        var lockObj = new object();
        var fileCounter = 0;
        var startPositions = new List<long> { 0L };

        // Determine chunk start positions
        using (var readerX = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            for (int i = 1; i < numThreads; i++)
            {
                long startPosition = i * chunkSize;
                readerX.Position = startPosition;
                int currentByte;

                while ((currentByte = readerX.ReadByte()) != -1 && currentByte != '\n')
                {
                    startPosition++;
                }
                startPosition++;
                startPositions.Add(startPosition);
            }
        }

        for (int i = 0; i < numThreads; i++)
        {
            var threadIndex = i;
            var task = Task.Run(async () =>
            {
                var threadOutputFiles = new List<string>();

                await using var reader = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.Asynchronous | FileOptions.SequentialScan);

                long startPosition = startPositions[threadIndex];
                reader.Position = startPosition;

                long endPosition = threadIndex == numThreads - 1 ? fileInfo.Length : startPositions[threadIndex + 1] - 1;

                using var rowBuffer = new MemoryStream(4096);

                var lineCount = 0;
                var chunkSize = 0L;
                List<ReadOnlyMemory<byte>> lines = new();

                while (reader.Position < endPosition || threadIndex == numThreads - 1)
                {
                    rowBuffer.SetLength(0);
                    rowBuffer.Position = 0;

                    var endOfFile = await ReadLineAsync(reader, rowBuffer);
                    if (endOfFile) break;

                    lines.Add(new ReadOnlyMemory<byte>(rowBuffer.ToArray(), 0, (int)rowBuffer.Position));

                    chunkSize += rowBuffer.Position;
                    lineCount++;

                    if (lineCount >= linesPerFile)
                    {
                        lines.Sort(_comparer);

                        string outputFile;
                        lock (lockObj)
                        {
                            outputFile = Path.Combine(tempOutputDir, $"chunk_{fileCounter++}.tmp");
                        }

                        await WriteChunkAsync(outputFile, lines, (int)chunkSize);

                        lines.Clear();
                        threadOutputFiles.Add(outputFile);
                        chunkSize = 0L;
                        lineCount = 0;
                    }
                }

                if (lines.Count > 0)
                {
                    string outputFile;
                    lock (lockObj)
                    {
                        outputFile = Path.Combine(tempOutputDir, $"chunk_{fileCounter++}.tmp");
                    }

                    lines.Sort(_comparer);
                    await WriteChunkAsync(outputFile, lines, (int)chunkSize);

                    lines.Clear();
                    threadOutputFiles.Add(outputFile);
                }

                return threadOutputFiles;
            });

            tasks.Add(task);
        }

        var chunkFiles = await Task.WhenAll(tasks);
        return chunkFiles.SelectMany(f => f).ToList();
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
                        var endOfFile = ReadLine(reader, rowBuffer);
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
                        var endOfFile = ReadLine(reader, rowBuffer);
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
                        var endOfFile = await ReadLineAsync(reader, rowBuffer);
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
                        var endOfFile = await ReadLineAsync(reader, rowBuffer);
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

    /// <summary>
    /// Writes the chunks.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    private void WriteChunks(string outputFile, List<ReadOnlyMemory<byte>> lines, int chunkSize)
    {
        using (var writer = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, chunkSize, FileOptions.WriteThrough))
        {
            foreach (var line in lines)
            {
                writer.Write(line.Span);
            }
            writer.Flush();
        }
    }


    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="outputFile">The output file.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A Task.</returns>
    private async Task WriteChunkAsync(string outputFile, List<ReadOnlyMemory<byte>> lines, int chunkSize)
    {
        await using var writer = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, chunkSize, FileOptions.Asynchronous | FileOptions.WriteThrough);

        foreach (var line in lines)
        {
            await writer.WriteAsync(line);
        }

        await writer.FlushAsync();
    }
    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="lineBuffer">The line buffer.</param>
    /// <returns>A bool.</returns>
    private bool ReadLine(Stream reader, Stream lineBuffer)
    {
        lineBuffer.SetLength(0);
        lineBuffer.Position = 0;

        int currentByte;

        while ((currentByte = reader.ReadByte()) != -1)
        {
            if (currentByte == '\n')
            {
                lineBuffer.WriteByte((byte)currentByte);
                break;
            }

            if (currentByte != '\r')
            {
                lineBuffer.WriteByte((byte)currentByte);
            }
        }

        var endOfFile = lineBuffer.Length == 0 && currentByte == -1;
        return endOfFile;
    }

    /// <summary>
    /// Reads the line async.
    /// </summary>
    /// <param name="readerStream">The reader stream.</param>
    /// <param name="lineBuffer">The line buffer.</param>
    /// <returns>A Task.</returns>
    private async Task<bool> ReadLineAsync(Stream readerStream, MemoryStream lineBuffer)
    {
        lineBuffer.SetLength(0);
        lineBuffer.Position = 0;

        int currentByte;
        var singleByte = new byte[1];

        while (await readerStream.ReadAsync(singleByte, 0, 1) > 0)
        {
            currentByte = singleByte[0];

            if (currentByte == '\n')
            {
                lineBuffer.WriteByte((byte)currentByte); 
                break;
            }

            if (currentByte != '\r')
            {
                lineBuffer.WriteByte((byte)currentByte);
            }
        }

        // Check for end of file
        bool endOfFile = lineBuffer.Length == 0 && singleByte[0] == 0; // && currentByte == -1;

        return endOfFile;
    }

}
