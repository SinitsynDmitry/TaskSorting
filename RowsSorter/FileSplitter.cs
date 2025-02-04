using RowsSorter.Interfaces;

namespace RowsSorter;

internal class FileSplitter: IFileSplitter
{
    private readonly IComparer<ReadOnlyMemory<byte>> _comparer;
    private readonly IStreamLineReader _streamLineReader;

    const int BUFFER_SIZE = 64 * 1024;
    const int MEMORY_BUFFER_SIZE = 4096;

    public FileSplitter()
    {
        _comparer = new ReadOnlyMemoryByteComparer();
        _streamLineReader = new StreamLineReader();
    }

    public FileSplitter(IComparer<ReadOnlyMemory<byte>> comparer, IStreamLineReader streamLineReader)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(streamLineReader);
        _comparer = comparer;
        _streamLineReader = streamLineReader;
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

                    var endOfFile = _streamLineReader.ReadLine(reader, rowBuffer);
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

                    var endOfFile = await _streamLineReader.ReadLineAsync(reader, rowBuffer);
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

}
