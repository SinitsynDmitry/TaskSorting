namespace RowsSorter.Shared;

public class TemporaryFileManager
{
    private readonly int _maxChunksPerTask;
    private readonly int _minChunksPerTask;

    private readonly string _tempOutputDir;
    private List<string> _chunks;
    public bool IsEmpty { get; private set; }

    public string OutputFile { get; private set; }

    public int Count => _chunks.Count;

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

    public void Clear()
    {
        _chunks.Clear();
    }

    public string GetTempFileName(int counter, int start, int end)
    {
        return Path.Combine(_tempOutputDir, $"chunk_{counter}_{start}_{end}.tmp");
    }

    private int CalculateOptimalPartSize(int totalChunks)
    {
        return Math.Clamp((int)Math.Sqrt(totalChunks), _minChunksPerTask, _maxChunksPerTask);
    }

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
