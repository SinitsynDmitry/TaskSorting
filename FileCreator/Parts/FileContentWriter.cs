using System.Runtime.CompilerServices;
using System.Text;
using FileCreator.Interfaces;

namespace FileCreator.Parts;

public class FileContentWriter : IContentWriter
{
    private readonly StreamWriter _writer;

    private readonly FileStream _fileStream;

    private bool _disposed;

    public FileContentWriter(string filePath, IGeneratorConfig config)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(config);

        _fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            config.BufferSize,
            useAsync: false
        );
        _writer = new StreamWriter(_fileStream, config.Encoding, config.BufferSize);
        _writer.AutoFlush = false;
    }

    /// <summary>
    /// Writes the buffer async.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ValueTask.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask WriteBufferAsync(StringBuilder buffer)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileContentWriter));

        if (buffer.Length > 0)
        {
            foreach (var chunk in buffer.GetChunks())
            {
                await _writer.WriteAsync(chunk);
            }

            buffer.Clear();
        }
    }

    /// <summary>
    /// Writes the buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBuffer(StringBuilder buffer)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileContentWriter));

        if (buffer.Length > 0)
        {
            foreach (var chunk in buffer.GetChunks())
            {
                _writer.Write(chunk);
            }

            buffer.Clear();
        }
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _writer.Dispose();
        _fileStream.Dispose();
        _disposed = true;
    }
}
