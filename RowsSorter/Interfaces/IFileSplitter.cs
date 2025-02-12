namespace RowsSorter.Interfaces;

/// <summary>
/// Handles splitting large files into smaller chunks
/// </summary>
public interface IFileSplitter: IDisposable
{
    /// <summary>
    /// Splits the file.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    /// <param name="tempOutputDir">The temp output dir.</param>
    /// <returns>A list of string.</returns>
    List<string> SplitFile(string inputFile, int linesPerFile, string tempOutputDir);

    /// <summary>
    /// Splits the file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="linesPerFile">The lines per file.</param>
    /// <param name="tempOutputDir">The temp output dir.</param>
    /// <returns>A Task.</returns>
    Task<List<string>> SplitFileAsync(string inputFile, int linesPerFile, string tempOutputDir);
}

