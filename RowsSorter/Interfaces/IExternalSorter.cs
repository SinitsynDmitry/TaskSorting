namespace RowsSorter.Interfaces;

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
    /// <param name="linesPerFile">The lines per file.</param>
    void SortLargeFile(string inputFile, string outputFile, int linesPerFile);
    //Task SortLargeFileAsync(string inputFile, string outputFile, int linesPerFile);
}
