﻿namespace RowsSorter.MainPipelineSteps.ChunkMerge.Parts;

public class TemporaryFileManager
{
    private readonly int _maxChunksPerTask;

    private readonly string _tempOutputDir;
    private ArraySegment<string> _chunks;

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
    /// Gets the max chunks per task.
    /// </summary>
    public int MaxChunksPerTask => _maxChunksPerTask;

    /// <summary>
    /// Gets the chunks.
    /// </summary>
    /// <returns>A list of string.</returns>
    public IReadOnlyList<string> GetChunks() => _chunks.AsReadOnly();

    public TemporaryFileManager(
        TempFileCollection files,
        int maxChunksPerTask = 30
    )
    {
        _tempOutputDir =
            Path.GetDirectoryName(files.Chunks[0])
            ?? throw new InvalidOperationException("Invalid temp directory.");
        OutputFile = files.OutputFile;
        _chunks = files.Chunks;
        _maxChunksPerTask = maxChunksPerTask;
    }

    /// <summary>
    /// Clears the.
    /// </summary>
    public void Clear()
    {
        _chunks=null;
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
       // return Math.Clamp((int)Math.Sqrt(totalChunks), _minChunksPerTask, _maxChunksPerTask);
        return Math.Min(_maxChunksPerTask, totalChunks);
    }

    /// <summary>
    /// Gets the next merge batch.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A list of TempFileCollections.</returns>
    public List<TempFileCollection> GetNextMergeBatch(int index = 0)
    {
        
        if (_chunks.Count < _maxChunksPerTask)
        {
            IsEmpty = true;
            return new List<TempFileCollection> { new() { Chunks = _chunks, OutputFile = OutputFile } };
        }

        var partSize = CalculateOptimalPartSize(_chunks.Count);
       
        var newChunks = new List<string>((int)Math.Ceiling(_chunks.Count / (double)partSize));
        var mergeBatches = new List<TempFileCollection>(newChunks.Capacity);

        for (int i = 0; i < _chunks.Count; i += partSize)
        {
            int end = Math.Min(i + partSize, _chunks.Count);
            var tempFile = GetTempFileName(index, i, end);
            mergeBatches.Add(new TempFileCollection { Chunks = _chunks.Slice(i, end - i), OutputFile = tempFile });

            newChunks.Add(tempFile);
        }

        _chunks = new ArraySegment<string>(newChunks.ToArray());

        return mergeBatches;
    }
}
