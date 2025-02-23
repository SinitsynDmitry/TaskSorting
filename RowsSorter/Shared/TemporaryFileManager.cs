namespace RowsSorter.Shared;

public class TemporaryFileManager
{
    private readonly int _maxChunksPerTask;
    private readonly int _minChunksPerTask;

    private readonly string _tempOutputDir;
    private List<string> _chunks;
    /// <summary>
    /// Gets a value indicating whether is empty.
    /// </summary>
    public bool IsEmpty { get; private set; }

    /// <summary>
    /// Gets the output file.
    /// </summary>
    public string OutputFile { get; private set; }

    /// <summary>
    /// Gets the count.
    /// </summary>
    public int Count => _chunks.Count;

    /// <summary>
    /// Gets the chunks.
    /// </summary>
    /// <returns>A list of string.</returns>
    public IReadOnlyList<string> GetChunks() => _chunks.AsReadOnly();

    public TemporaryFileManager(
        TempFileCollection files,
        int maxChunksPerTask = 30,
        int minChunksPerTask = 2
    )
    {
        _tempOutputDir =
            Path.GetDirectoryName(files.Chunks[0])
            ?? throw new InvalidOperationException("Invalid temp directory.");
        OutputFile = files.OutputFile;
        _chunks = files.Chunks;
        _maxChunksPerTask = maxChunksPerTask;
        _minChunksPerTask = minChunksPerTask;
    }

    /// <summary>
    /// Clears the.
    /// </summary>
    public void Clear()
    {
        _chunks.Clear();
    }

    /// <summary>
    /// Gets the temp file name.
    /// </summary>
    /// <param name="counter">The counter.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <returns>A string.</returns>
    public string GetTempFileName(int counter, int start, int end)
    {
        return Path.Combine(_tempOutputDir, $"chunk_{counter}_{start}_{end}.tmp");
    }

    /// <summary>
    /// Calculates the optimal part size.
    /// </summary>
    /// <param name="totalChunks">The total chunks.</param>
    /// <returns>An int.</returns>
    private int CalculateOptimalPartSize(int totalChunks)
    {
        return Math.Clamp((int)Math.Sqrt(totalChunks), _minChunksPerTask, _maxChunksPerTask);
    }

    /// <summary>
    /// Gets the next merge batch.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A list of TempFileCollections.</returns>
    public List<TempFileCollection> GetNextMergeBatch(int index = 0)
    {
        var mergeBatches = new List<TempFileCollection>();
        if (_chunks.Count < _maxChunksPerTask)
        {
            mergeBatches.Add(new TempFileCollection { Chunks = _chunks, OutputFile = OutputFile });
            IsEmpty = true;
            return mergeBatches;
        }

        var partSize = CalculateOptimalPartSize(_chunks.Count);

        var newChunks = new List<string>((int)Math.Ceiling(_chunks.Count / (double)partSize));

        for (int i = 0; i < _chunks.Count; i += partSize)
        {
            int end = Math.Min(i + partSize, _chunks.Count);
            var chunkRange = _chunks.Skip(i).Take(end - i).ToList();
            var tempFile = GetTempFileName(index, i, end);
            mergeBatches.Add(new TempFileCollection { Chunks = chunkRange, OutputFile = tempFile });

            newChunks.Add(tempFile);
        }

        _chunks = newChunks;

        return mergeBatches;
    }
}
