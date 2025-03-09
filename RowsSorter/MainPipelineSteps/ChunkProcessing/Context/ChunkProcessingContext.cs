using System.Buffers;

namespace RowsSorter.MainPipelineSteps.ChunkProcessing.Context;

public class ChunkProcessingContext
{
    private ChunkProcessingContext(
        string inputFile,
        string outputFile,
        long startPosition,
        int chunkSize,
        byte[] buffer
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
        ArgumentOutOfRangeException.ThrowIfNegative(startPosition);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize, 0);

        InputFile = inputFile;
        OutputFile = outputFile;
        StartPosition = startPosition;
        ChunkSize = chunkSize;
        Buffer = buffer;
    }

    /// <summary>
    /// Gets or sets the buffer.
    /// </summary>
    public byte[] Buffer { get; set; }

    /// <summary>
    /// Gets or sets the bytes read.
    /// </summary>
    public int BytesRead { get; set; }

    /// <summary>
    /// Gets the chunk size.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the input file.
    /// </summary>
    public string InputFile { get; }

    /// <summary>
    /// Gets the output file.
    /// </summary>
    public string OutputFile { get; }

    /// <summary>
    /// Gets the start position.
    /// </summary>
    public long StartPosition { get; }

    /// <summary>
    /// Creates the.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A ChunkProcessingContext.</returns>
    public static ChunkProcessingContext Create(
        string inputFile,
        string outputFile,
        long startPosition,
        int chunkSize
    )
    {
        var buffer = ArrayPool<byte>.Shared.Rent(chunkSize);

        return new ChunkProcessingContext(inputFile, outputFile, startPosition, chunkSize, buffer);
    }

    /// <summary>
    /// Releases the buffer.
    /// </summary>
    public void ReleaseBuffer()
    {
        if (Buffer != null)
        {
            ArrayPool<byte>.Shared.Return(Buffer, true);
            Buffer = null;
        }
    }
}
