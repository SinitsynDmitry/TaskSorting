namespace RowsSorter.Interfaces;

    /// <summary>
    /// Handles splitting large files into smaller chunks
    /// </summary>
    public interface IFileSplitter
    {
        List<string> SplitFile(string inputFile, int linesPerFile, string tempOutputDir);
        Task<List<string>> SplitFileAsync(string inputFile, int linesPerFile, string tempOutputDir);
    }

