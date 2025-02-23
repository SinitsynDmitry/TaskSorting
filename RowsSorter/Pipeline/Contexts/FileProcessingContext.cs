namespace RowsSorter.Pipeline.Contexts;

public class FileProcessingContext
{
    public FileProcessingContext(
        string inputFile,
        string outputFile,
        int baseChunkSize,
        string tempOutputDir
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(tempOutputDir);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(baseChunkSize, 0);

        InputFile = inputFile;
        BaseChunkSize = baseChunkSize;
        TempOutputDir = tempOutputDir;
        OutputFile = outputFile;
    }

    /// <summary>
    /// Gets the base chunk size.
    /// </summary>
    public int BaseChunkSize { get; }

    /// <summary>
    /// Gets the input file.
    /// </summary>
    public string InputFile { get; }

    /// <summary>
    /// Gets the output file.
    /// </summary>
    public string OutputFile { get; }

    /// <summary>
    /// Gets the output files.
    /// </summary>
    public List<(long startPosition, int chunkSize, string outputFile)> TempChunks { get; } = new();

    /// <summary>
    /// Gets the temp output dir.
    /// </summary>
    public string TempOutputDir { get; }
}
