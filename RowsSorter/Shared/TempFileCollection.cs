namespace RowsSorter.Shared;

public class TempFileCollection
{
    /// <summary>
    /// Gets or sets the chunks.
    /// </summary>
    public ArraySegment<string> Chunks { get; set; }

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
