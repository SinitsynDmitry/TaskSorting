using RowsSorter.Extensions;

namespace RowsSorter.Extensions;

public static class StreamExtensions
{
    /// <summary>
    /// Indices the of.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="value">The value.</param>
    /// <param name="buffer">The search _buffer.</param>
    /// <returns>A long.</returns>
    public static long IndexOf(
        this Stream stream,
        long startPosition,
        byte value,
        Span<byte> buffer
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
        bool separatorFound = false;

        while (!separatorFound)
        {
            int bytesRead = stream.Read(buffer);
            if (bytesRead == 0)
                return -1;

            int separatorIndex = buffer.IndexOf(value);

            if (separatorIndex != -1)
            {
                return currentPosition + separatorIndex + 1;
            }

            currentPosition += bytesRead;
        }

        return -1;
    }

    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>A ReadOnlyMemory&lt;byte&gt;? .</returns>
    public static ReadOnlyMemory<byte>? ReadLine(
        this Stream stream,
        Span<byte> buffer,
        byte separator = (byte)'\n'
    )
    {
        ArgumentNullException.ThrowIfNull(stream);

        long streamPositionBeforeRead = stream.Position;
        int bytesRead = stream.Read(buffer);
        if (bytesRead == 0)
        {
            return null;
        }

        var chunkBytes = buffer.Slice(0, bytesRead);
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
    /// Finds the chunk start positions.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>A list of long.</returns>
    public static IReadOnlyList<long> FindChunkStartPositions(
        this Stream stream,
        int partsCount,
        long chunkSize,
        byte separator
    )
    {
        const int searchBufferSize = 128;
        Span<byte> searchBuffer = stackalloc byte[searchBufferSize];

        List<long> startPositions = new() { 0L };

        for (int i = 1; i < partsCount; i++)
        {
            long startPosition = i * chunkSize;
            long correctedPosition = stream.IndexOf(startPosition, separator, searchBuffer);

            startPositions.Add(correctedPosition > 0 ? correctedPosition : startPosition);
        }

        return startPositions;
    }
}
