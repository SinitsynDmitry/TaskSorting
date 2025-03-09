namespace RowsSorter.Core;

/// <summary>
/// Coordinates the complete external sort operation
/// </summary>
public interface IExternalMergeSorter
{

    /// <summary>
    /// Sorts the large file.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="baseChunkSize">The base chunk size.</param>
    void SortLargeFile(string inputFile, string outputFile, int baseChunkSize);


    /// <summary>
    /// Sorts the large file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="baseChunkSize">The base chunk size.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask SortLargeFileAsync(string inputFile, string outputFile, int baseChunkSize);
}
