using System.Buffers;
using RowsSorter.Interfaces;

namespace RowsSorter.Shared;

internal class FileStreamProvider : IFileStreamProvider
{
    private readonly ArrayPool<byte> _bufferPool;
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStreamProvider"/> class.
    /// </summary>
    public FileStreamProvider()
    {
        _bufferPool = ArrayPool<byte>.Shared;
    }

    /// <summary>
    /// Gets the read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A FileStream.</returns>
    public FileStream GetReadStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false)
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
    /// Gets the pooled read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="isAsync">If true, is async.</param>
    /// <returns>A PooledFileStream.</returns>
    public PooledFileStream GetPooledReadStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false)
    {
        var buffer = _bufferPool.Rent(bufferSize);

        var stream = GetReadStream(filePath, bufferSize, isAsync);

        return new PooledFileStream(stream, buffer);
    }

    /// <summary>
    /// Gets the write stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A Stream.</returns>
    public Stream GetWriteStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false)
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

    /// <summary>
    /// Deletes the files.
    /// </summary>
    /// <param name="files">The files.</param>
    public void DeleteFiles(IReadOnlyList<string> files)
    {
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }
}
