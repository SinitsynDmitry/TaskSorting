namespace RowsSorter.Interfaces;

public interface IStreamProvider
{
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// Gets the read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A FileStream.</returns>
    Stream GetReadStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false);

    /// <summary>
    /// Gets the write stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A Stream.</returns>
    Stream GetWriteStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false);

}
