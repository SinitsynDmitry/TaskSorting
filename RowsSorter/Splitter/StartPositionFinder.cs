using RowsSorter.Extensions;
using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Splitter;

internal class StartPositionFinder : IStartPositionFinder
{
    private readonly byte _separator;
    private readonly IFileStreamProvider _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartPositionFinder"/> class.
    /// </summary>
    /// <param name="separator">The separator.</param>
    public StartPositionFinder(byte separator = (byte)'\n')
    {
        _streamProvider = new FileStreamProvider();
        _separator = separator;
    }

    /// <summary>
    /// Determines the chunk start positions.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A list of long.</returns>
    public IReadOnlyList<long> DetermineChunkStartPositions(string inputFile, int partsCount, long chunkSize)
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, (int)chunkSize);

        return fileStream.DetermineChunkStartPositions(partsCount, chunkSize, _separator);
    }

    /// <summary>
    /// Determines the chunk start positions async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A Task.</returns>
    public async ValueTask<IReadOnlyList<long>> DetermineChunkStartPositionsAsync(string inputFile, int partsCount, long chunkSize)
    {
        using var fileStream = _streamProvider.GetReadStream(inputFile, (int)chunkSize, true);

        return await fileStream.DetermineChunkStartPositionsAsync(partsCount, chunkSize, _separator);
    }
}