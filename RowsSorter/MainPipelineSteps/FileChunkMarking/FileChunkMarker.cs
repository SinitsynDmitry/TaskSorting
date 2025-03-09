using RowsSorter.Common.Interfaces;
using RowsSorter.Extensions;

namespace RowsSorter.MainPipelineSteps.FileChunkMarking;

public class FileChunkMarker : IFileChunkMarker
{
    private readonly byte _separator;
    private readonly IStreamProvider _streamProvider;

    public FileChunkMarker(IStreamProvider streamProvider, byte separator = (byte)'\n')
    {
        ArgumentNullException.ThrowIfNull(streamProvider);
        _streamProvider = streamProvider;
        _separator = separator;
    }

    /// <summary>
    /// Finds the chunk start positions.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A list of long.</returns>
    public IReadOnlyList<long> FindChunksStartPositions(
        string inputFile,
        int partsCount,
        long chunkSize
    )
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, (int)chunkSize);

        return fileStream.FindChunkStartPositions(partsCount, chunkSize, _separator);
    }

    /// <summary>
    /// Finds the chunk start positions async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask<IReadOnlyList<long>> FindChunkStartsPositionsAsync(
        string inputFile,
        int partsCount,
        long chunkSize
    )
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, (int)chunkSize, true);

        return await fileStream.FindChunkStartPositionsAsync(partsCount, chunkSize, _separator);
    }
}
