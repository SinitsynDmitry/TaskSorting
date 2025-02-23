namespace RowsSorter.Shared;

public class TempFileCollection
{
    /// <summary>
    /// Gets or sets the chunks.
    /// </summary>
    public List<string> Chunks { get; set; } = new();
    /// <summary>
    /// Gets or sets the output file.
    /// </summary>
    public string OutputFile { get; set; } = string.Empty;

    /// <summary>
    /// Deletes the files.
    /// </summary>
    public void DeleteFiles()
    {
        foreach (var file in Chunks)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
