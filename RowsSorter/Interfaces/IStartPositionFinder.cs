namespace RowsSorter.Interfaces;

public interface IStartPositionFinder
{

    /// <summary>
    /// Determines the chunk start positions.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A list of long.</returns>
    IReadOnlyList<long> DetermineChunkStartPositions(string inputFile, int partsCount, long chunkSize);

    /// <summary>
    /// Determines the chunk start positions async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A Task.</returns>
    ValueTask<IReadOnlyList<long>> DetermineChunkStartPositionsAsync(string inputFile, int partsCount, long chunkSize);
}