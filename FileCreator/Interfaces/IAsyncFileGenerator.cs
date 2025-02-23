namespace FileCreator.Interfaces;

public interface IAsyncFileGenerator 
{

    /// <summary>
    /// Generates the file async.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask GenerateFileAsync(IContentWriter writer, long rowsInFile);
}

