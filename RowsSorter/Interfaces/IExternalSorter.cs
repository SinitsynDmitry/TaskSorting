namespace RowsSorter.Interfaces;

/// <summary>
/// Coordinates the complete external sort operation
/// </summary>
public interface IExternalMergeSorter
{
    void SortLargeFile(string inputFile, string outputFile, int linesPerFile);
    Task SortLargeFileAsync(string inputFile, string outputFile, int linesPerFile);
}
