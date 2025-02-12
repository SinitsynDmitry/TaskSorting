using RowsSorter.Shared;

namespace RowsSorter.Interfaces;

internal interface IFileStreamProvider
{
    private const int BUFFER_SIZE = 4096;

    /// <summary>
    /// Gets the read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A FileStream.</returns>
    FileStream GetReadStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false);

    /// <summary>
    /// Gets the pooled read stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="bufferSize">The buffer size.</param>
    /// <param name="isAsync">If true, is async.</param>
    /// <returns>A PooledFileStream.</returns>
    PooledFileStream GetPooledReadStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false);

    /// <summary>
    /// Gets the write stream.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A Stream.</returns>
    Stream GetWriteStream(string filePath, int bufferSize = BUFFER_SIZE, bool isAsync = false);

    /// <summary>
    /// Deletes the files.
    /// </summary>
    /// <param name="files">The files.</param>
    void DeleteFiles(IReadOnlyList<string> files);
}
