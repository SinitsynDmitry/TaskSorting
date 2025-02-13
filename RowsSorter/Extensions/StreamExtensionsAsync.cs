using System.Buffers;
using RowsSorter.Entities;
using RowsSorter.Extensions;

namespace RowsSorter.Extensions;
public static class StreamExtensionsAsync
{
    /// <summary>
    /// Asynchronously finds the index of a specific byte value in a stream starting from a given position.
    /// </summary>
    /// <param name="stream">The stream to search</param>
    /// <param name="startPosition">The starting position in the stream</param>
    /// <param name="value">The byte value to search for</param>
    /// <param name="buffer">A buffer used for reading chunks of the stream</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The absolute position of the byte in the stream, or -1 if not found</returns>
    public static async Task<long> IndexOfAsync(
        this Stream stream,
        long startPosition,
        byte value,
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegative(startPosition);

        if (!stream.CanSeek)
            throw new ArgumentException("Stream must support seeking", nameof(stream));
        if (!stream.CanRead)
            throw new ArgumentException("Stream must support reading", nameof(stream));

        stream.Seek(startPosition, SeekOrigin.Begin);
        long currentPosition = startPosition;

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
                return -1;

            var chunk = buffer.Span.Slice(0, bytesRead);
            int separatorIndex = chunk.IndexOf(value);

            if (separatorIndex != -1)
            {
                return currentPosition + separatorIndex;
            }

            currentPosition += bytesRead;
        }

        return -1;
    }

    /// <summary>
    /// Asynchronously reads a line from the stream using a provided buffer.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="buffer">A buffer for reading stream contents</param>
    /// <param name="chunkSize">The maximum size of the chunk to read</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A ReadOnlyMemory<byte> representing the line, or null if no line is found</returns>
    public static async Task<ReadOnlyMemory<byte>?> ReadLineAsync(
        this Stream stream,
        Memory<byte> buffer,
        int chunkSize = 512,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (chunkSize > buffer.Length)
            throw new ArgumentException("ChunkSize cannot be larger than buffer length", nameof(chunkSize));

        long streamPositionBeforeRead = stream.Position;
        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead == 0)
        {
            return null;
        }

        var chunkBytes = buffer.Span.Slice(0, bytesRead);
        int separatorIndex = chunkBytes.IndexOf((byte)'\n');

        if (separatorIndex == -1)
        {
            return null;
        }

        // Create a copy of the line including the newline character
        var lineCopy = chunkBytes.Slice(0, separatorIndex + 1).ToArray();

        // Adjust stream position to after the line
        stream.Position = streamPositionBeforeRead + separatorIndex + 1;

        return lineCopy;
    }

    /// <summary>
    /// Reads the chunk async.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ValueTask.</returns>
    public static async ValueTask<ReadOnlyMemory<byte>> ReadChunkAsync(this Stream stream, long startPosition, int chunkSize, byte[]? buffer = null)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException("Stream is not readable.");

        if (!stream.CanSeek)
            throw new InvalidOperationException("Stream does not support seeking.");

        ArgumentOutOfRangeException.ThrowIfNegative(startPosition);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize,0);
       

        buffer ??= new byte[chunkSize];
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, buffer.Length);

        stream.Seek(startPosition, SeekOrigin.Begin);
        int bytesRead = await stream.ReadAsync(buffer, 0, chunkSize);

        return new ReadOnlyMemory<byte>(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task.</returns>
    public static async ValueTask WriteChunkAsync(this Stream stream, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must support writing", nameof(stream));
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize, 0);

        buffer ??= new byte[chunkSize];
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, buffer.Length);

        int bufferOffset = 0;
        foreach (var line in lines)
        {
            if (bufferOffset + line.Length > buffer.Length)
            {
                await stream.WriteAsync(buffer.AsMemory(0, bufferOffset), cancellationToken);
                bufferOffset = 0;
            }

            line.CopyToSpan(buffer.AsSpan(bufferOffset));
            bufferOffset += line.Length;
        }
        if (bufferOffset > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bufferOffset), cancellationToken);
        }
    }

    /// <summary>
    /// Determines the chunk start positions async.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>A ValueTask.</returns>
    public static async ValueTask<IReadOnlyList<long>> DetermineChunkStartPositionsAsync(this Stream stream, int partsCount, long chunkSize,byte separator)
    {
        const int searchBufferSize = 128;
        var arrayPool = ArrayPool<byte>.Shared;
        var searchBuffer = arrayPool.Rent(searchBufferSize);

        try
        {
            List<long> startPositions = new() { 0L };

            for (int i = 1; i < partsCount; i++)
            {
                long startPosition = i * chunkSize;
                long currentPosition = await stream.IndexOfAsync(startPosition, separator, searchBuffer);

                if (currentPosition > 0)
                {
                    startPositions.Add(currentPosition);
                }
            }

            return startPositions.AsReadOnly();
        }
        finally
        {
            arrayPool.Return(searchBuffer, clearArray: true);
        }
    }
}
