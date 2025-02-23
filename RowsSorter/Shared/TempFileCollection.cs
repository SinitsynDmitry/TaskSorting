namespace RowsSorter.Shared;

public class TempFileCollection
{
    public List<string> Chunks { get; set; } = new();
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
