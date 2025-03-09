using FileCreator.Interfaces;

namespace FileCreator.Core;

public interface IFileGenerator
{

    /// <summary>
    /// Generates the file.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    void GenerateFile(IContentWriter writer, long rowsInFile);
}

