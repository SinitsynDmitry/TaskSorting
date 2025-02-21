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
    /// <param name="buffer">A _buffer used for reading chunks of the stream</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The absolute position of the byte in the stream, or -1 if not found</returns>
    public static async Task<long> IndexOfAsync(
        this Stream stream,
        long startPosition,
        byte value,
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    )
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
                return currentPosition + separatorIndex + 1;
            }

            currentPosition += bytesRead;
        }

        return -1;
    }

    /// <summary>
    /// Reads the line async.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task.</returns>
    public static async Task<ReadOnlyMemory<byte>?> ReadLineAsync(
        this Stream stream,
        Memory<byte> buffer,
        byte separator = (byte)'\n',
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);

        long streamPositionBeforeRead = stream.Position;
        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        if (bytesRead == 0)
        {
            return null;
        }

        var chunkBytes = buffer.Span.Slice(0, bytesRead);
        int separatorIndex = chunkBytes.IndexOf(separator);

        if (separatorIndex == -1)
        {
            return null;
        }

        var lineCopy = chunkBytes.Slice(0, separatorIndex + 1).ToArray();

        stream.Position = streamPositionBeforeRead + separatorIndex + 1;

        return lineCopy;
    }

    /// <summary>
    /// Finds the chunk start positions async.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueTask.</returns>
    public static async ValueTask<IReadOnlyList<long>> FindChunkStartPositionsAsync(
        this Stream stream,
        int partsCount,
        long chunkSize,
        byte separator,
        CancellationToken cancellationToken = default
    )
    {
        const int searchBufferSize = 128;
        List<long> startPositions = new() { 0L };

        // Allocate stack buffer
        byte[] buffer = GC.AllocateUninitializedArray<byte>(searchBufferSize);

        for (int i = 1; i < partsCount; i++)
        {
            long startPosition = i * chunkSize;

            long currentPosition = await stream.IndexOfAsync(
                startPosition,
                separator,
                buffer,
                cancellationToken
            );

            if (currentPosition > 0)
            {
                startPositions.Add(currentPosition);
            }
        }

        return startPositions;
    }
}
