using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Pipeline.Steps;

public class FileChunkMarkingStep : IProcessingStep<FileProcessingContext>
{
    private readonly IFileChunkMarker _fileChunkMarker;

    public FileChunkMarkingStep(IFileChunkMarker fileChunkMarker)
    {
        _fileChunkMarker = fileChunkMarker;
    }

    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ProcessAsync(FileProcessingContext context)
    {
        var fileInfo = new FileInfo(context.InputFile);

        int partsCount = (int)Math.Ceiling((double)fileInfo.Length / context.BaseChunkSize);

        var startPositions = await _fileChunkMarker.FindChunkStartsPositionsAsync(
            context.InputFile,
            partsCount,
            context.BaseChunkSize
        );
        AddChunksRange(context, fileInfo, partsCount, startPositions);
    }

    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Process(FileProcessingContext context)
    {
        var fileInfo = new FileInfo(context.InputFile);
        int partsCount = (int)Math.Ceiling((double)fileInfo.Length / context.BaseChunkSize);

        var startPositions = _fileChunkMarker.FindChunksStartPositions(
            context.InputFile,
            partsCount,
            context.BaseChunkSize
        );
        AddChunksRange(context, fileInfo, partsCount, startPositions);
    }

    /// <summary>
    /// Adds the chunks range.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="fileInfo">The file info.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="startPositions">The start positions.</param>
    private static void AddChunksRange(
        FileProcessingContext context,
        FileInfo fileInfo,
        int partsCount,
        IReadOnlyList<long> startPositions
    )
    {
        context.TempChunks.AddRange(
            startPositions.Select(
                (pos, i) =>
                {
                    long startPosition = startPositions[i];

                    long endPosition =
                        i == partsCount - 1 ? fileInfo.Length : startPositions[i + 1];

                    int currentChunkSize = (int)(endPosition - startPosition);
                    var outputChunkFile = Path.Combine(context.TempOutputDir, $"chunk_{i}.tmp");

                    return (startPosition, currentChunkSize, outputChunkFile);
                }
            )
        );
    }
}
