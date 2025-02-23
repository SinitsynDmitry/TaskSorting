using RowsSorter.Interfaces;

namespace RowsSorter.Shared;

public class FileStreamProvider : IStreamProvider
{
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// Gets the read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A FileStream.</returns>
    public Stream GetReadStream(
        string filePath,
        int bufferSize = BUFFER_SIZE,
        bool isAsync = false
    )
    {
        FileOptions options = FileOptions.SequentialScan;
        if (isAsync)
        {
            options |= FileOptions.Asynchronous;
        }

        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            options
        );

        return stream;
    }

    /// <summary>
    /// Gets the write stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A Stream.</returns>
    public Stream GetWriteStream(
        string filePath,
        int bufferSize = BUFFER_SIZE,
        bool isAsync = false
    )
    {
        FileOptions options = FileOptions.WriteThrough;
        if (isAsync)
        {
            options |= FileOptions.Asynchronous;
        }

        return new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            BUFFER_SIZE,
            options
        );
    }
}
