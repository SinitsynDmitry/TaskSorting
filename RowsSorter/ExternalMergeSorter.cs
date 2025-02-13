using System.Collections.Concurrent;
using RowsSorter.Interfaces;
using RowsSorter.Merger;
using RowsSorter.Splitter;

namespace RowsSorter;
public class ExternalMergeSorter : IExternalMergeSorter
{
    private readonly IFileSplitter _fileSplitter;
    private readonly IChunkMerger _chunkMerger;

    const int MERGE_CYCLE = 30;


    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMergeSorter"/> class.
    /// </summary>
    public ExternalMergeSorter()
    {
        _chunkMerger = new ChunkMerger();
        _fileSplitter = new FileSplitter();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMergeSorter"/> class.
    /// </summary>
    /// <param name="fileSplitter">The file splitter.</param>
    /// <param name="chunkMerger">The chunk merger.</param>
    public ExternalMergeSorter(IFileSplitter fileSplitter, IChunkMerger chunkMerger)
    {
        ArgumentNullException.ThrowIfNull(fileSplitter);
        ArgumentNullException.ThrowIfNull(chunkMerger);

        _fileSplitter = fileSplitter;
        _chunkMerger = chunkMerger;
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

        var chunks = _fileSplitter.SplitFile(inputFile, linesPerFile, tempOutputDir);

        var counter = 0;
        while (chunks.Count > MERGE_CYCLE)
        {
            int partSize = (int)Math.Sqrt(chunks.Count);
            var rangePartitioner = Partitioner.Create(0, chunks.Count, partSize);
            var chunkFilesInternal = new ConcurrentBag<string>();

            Parallel.ForEach(rangePartitioner, range =>
            {
                var chunkRange = chunks.GetRange(range.Item1, range.Item2 - range.Item1);
                var temp = Path.Combine(tempOutputDir, $"chunk_{counter}_{range.Item1}_{range.Item2}.tmp");
                _chunkMerger.MergeSortedChunks(chunkRange, temp);
                chunkFilesInternal.Add(temp);
            });

            chunks = chunkFilesInternal.ToList();
            counter++;
        }

        _chunkMerger.MergeSortedChunks(chunks, outputFile);
    }

    /// <summary>
    /// Sorts the large file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    /// <returns>A Task.</returns>
    public async ValueTask SortLargeFileAsync(string inputFile, string outputFile, int linesPerFile)
    {
        var tempOutputDir = Path.Combine(Path.GetDirectoryName(outputFile)!, "temp");
        if (!Directory.Exists(tempOutputDir))
        {
            Directory.CreateDirectory(tempOutputDir);
        }

        var chunks = await _fileSplitter.SplitFileAsync(inputFile, linesPerFile, tempOutputDir);

        while (chunks.Count > MERGE_CYCLE)
        {
            int partSize = (int)Math.Sqrt(chunks.Count);
            var rangePartitioner = Partitioner.Create(0, chunks.Count, partSize).GetDynamicPartitions();
            var chunkFilesInternal = new ConcurrentBag<string>();
            var mergeTasks = new List<Task>();
            foreach (var range in rangePartitioner)
            {
                mergeTasks.Add(Task.Run(async () =>
                {
                    var chunkRange = chunks.GetRange(range.Item1, range.Item2 - range.Item1);
                    var temp = Path.Combine(tempOutputDir, $"chunk_{range.Item1}_{range.Item2}.tmp");
                    await _chunkMerger.MergeSortedChunksAsync(chunkRange, temp);
                    chunkFilesInternal.Add(temp);
                }));
            }

            await Task.WhenAll(mergeTasks);

            chunks = chunkFilesInternal.ToList();
        }

        await _chunkMerger.MergeSortedChunksAsync(chunks, outputFile);
    }
}
