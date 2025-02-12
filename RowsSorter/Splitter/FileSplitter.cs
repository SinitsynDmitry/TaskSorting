using System.Buffers;
using RowsSorter.Entities;
using RowsSorter.Extensions;
using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Splitter;

internal class FileSplitter : IFileSplitter
{
    private readonly byte _separator = (byte)'\n';
    private readonly IChunkReader _chunkReader;
    private readonly ILineSorter _lineSorter;
    private readonly IChunkWriter _chunkWriter;
    private readonly IStartPositionFinder _startPositionFinder;
    private readonly ThreadLocal<List<ByteChunkData>> _reusableLines =
    new(() => new List<ByteChunkData>());

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSplitter"/> class.
    /// </summary>
    public FileSplitter()
    {
        _startPositionFinder = new StartPositionFinder(_separator);
        _chunkWriter = new ChunkWriter();
        _lineSorter = new LineSorter(new ChunkDataComparer());
        _chunkReader = new ChunkReader();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSplitter"/> class.
    /// </summary>
    /// <param name="chunkReader">The chunk reader.</param>
    /// <param name="lineSorter">The line sorter.</param>
    /// <param name="chunkWriter">The chunk writer.</param>
    /// <param name="startPositionFinder">The start position finder.</param>
    public FileSplitter(
        IChunkReader chunkReader,
        ILineSorter lineSorter,
        IChunkWriter chunkWriter,
        IStartPositionFinder startPositionFinder
        )
    {
        _chunkReader = chunkReader ?? throw new ArgumentNullException(nameof(chunkReader));
        _lineSorter = lineSorter ?? throw new ArgumentNullException(nameof(lineSorter));
        _chunkWriter = chunkWriter ?? throw new ArgumentNullException(nameof(chunkWriter));
        _startPositionFinder = startPositionFinder ?? throw new ArgumentNullException(nameof(startPositionFinder));
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
        const int chunkSize = 4 * 1024 * 1024; // 4 MB
        int partsCount = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);
        var tasks = new List<Task<string>>();

        var startPositions = _startPositionFinder.DetermineChunkStartPositions(inputFile, partsCount, chunkSize);

        for (int i = 0; i < partsCount; i++)
        {
            var threadIndex = i;
            var task = Task.Run(() =>
            {
                long startPosition = startPositions[threadIndex];
                long endPosition = threadIndex == partsCount - 1
                    ? fileInfo.Length
                    : startPositions[threadIndex + 1];
                int currentChunkSize = (int)(endPosition - startPosition);
                var outputChunkFile = Path.Combine(tempOutputDir, $"chunk_{threadIndex}.tmp");

                ProcessChunk(inputFile, outputChunkFile, startPosition, currentChunkSize);

                return outputChunkFile;
            });

            tasks.Add(task);
        }

        return Task.WhenAll(tasks).GetAwaiter().GetResult().ToList();
    }

    /// <summary>
    /// Processes the chunk.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputChunkFile">The output chunk file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    private void ProcessChunk(string inputFile, string outputChunkFile, long startPosition, int chunkSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var bufferRead = arrayPool.Rent(chunkSize);
        var bufferWrite = arrayPool.Rent(chunkSize);
        try
        {
            var threadLocalLines = _reusableLines.Value;
            threadLocalLines!.Clear();

            var chunkBytes = _chunkReader.ReadChunk(inputFile, startPosition, chunkSize, bufferRead);

            chunkBytes.SplitIntoLines(threadLocalLines);
            _lineSorter.Sort(threadLocalLines);
            _chunkWriter.WriteChunk(outputChunkFile, threadLocalLines, chunkSize, bufferWrite);
        }
        finally
        {
            arrayPool.Return(bufferRead, clearArray: true);
            arrayPool.Return(bufferWrite, clearArray: true);
        }
    }

    /// <summary>
    /// Split file into sorted chunks
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="linesPerFile"></param>
    /// <param name="tempOutputDir"></param>
    /// <returns></returns>
    public async Task<List<string>> SplitFileAsync(string inputFile, int linesPerFile, string tempOutputDir)
    {
        var fileInfo = new FileInfo(inputFile);
        const int chunkSize = 4 * 1024 * 1024; // 1 MB
        int partsCount = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);
        var tasks = new List<Task<string>>();

        var startPositions = await _startPositionFinder.DetermineChunkStartPositionsAsync(inputFile, partsCount, chunkSize);

        for (int i = 0; i < partsCount; i++)
        {
            var threadIndex = i;
            var task = Task.Run(async () =>
            {
                long startPosition = startPositions[threadIndex];
                long endPosition = threadIndex == partsCount - 1
                    ? fileInfo.Length
                    : startPositions[threadIndex + 1];
                int currentChunkSize = (int)(endPosition - startPosition);
                var outputChunkFile = Path.Combine(tempOutputDir, $"chunk_{threadIndex}.tmp");

                await ProcessChunkAsync(inputFile, outputChunkFile, startPosition, currentChunkSize);

                return outputChunkFile;
            });

            tasks.Add(task);
        }

        return Task.WhenAll(tasks).GetAwaiter().GetResult().ToList();
    }

    /// <summary>
    /// Processes the chunk.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputChunkFile">The output chunk file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    private async ValueTask ProcessChunkAsync(string inputFile, string outputChunkFile, long startPosition, int chunkSize)
    {
        var arrayPool = ArrayPool<byte>.Shared;
        var bufferRead = arrayPool.Rent(chunkSize);
        var bufferWrite = arrayPool.Rent(chunkSize);
        try
        {
            var chunkBytes = await _chunkReader.ReadChunkAsync(inputFile, startPosition, chunkSize, bufferRead);

            var threadLocalLines = _reusableLines.Value;
            threadLocalLines!.Clear();

            chunkBytes.SplitIntoLines(threadLocalLines);
            _lineSorter.Sort(threadLocalLines);

            await _chunkWriter.WriteChunkAsync(outputChunkFile, threadLocalLines, chunkSize, bufferWrite);
        }
        finally
        {
            arrayPool.Return(bufferRead, clearArray: true);
            arrayPool.Return(bufferWrite, clearArray: true);
        }
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    /// <param name="disposing">If true, disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _reusableLines?.Dispose();
        }
    }
}