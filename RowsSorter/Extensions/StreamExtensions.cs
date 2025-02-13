using System.Buffers;
using RowsSorter.Entities;
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
    /// <param name="buffer">The search buffer.</param>
    /// <returns>A long.</returns>
    public static long IndexOf(this Stream stream, long startPosition, byte value, Span<byte> buffer)
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
                currentPosition += separatorIndex + 1;
                separatorFound = true;
            }
            else
            {
                currentPosition += bytesRead;
            }
        }

        return currentPosition;
    }

    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A ReadOnlyMemory&lt;byte&gt;? .</returns>
    public static ReadOnlyMemory<byte>? ReadLine(this Stream stream, Span<byte> buffer, int chunkSize = 512)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (chunkSize > buffer.Length)
            throw new ArgumentException("ChunkSize cannot be larger than buffer length", nameof(chunkSize));

        long streamPositionBeforeRead = stream.Position;
        int bytesRead = stream.Read(buffer);
        if (bytesRead == 0)
        {
            return null;
        }

        var chunkBytes = buffer.Slice(0, bytesRead);
        int separatorIndex = chunkBytes.IndexOf((byte)'\n');
        if (separatorIndex == -1)
        {
            return null;
        }

        var lineCopy = chunkBytes.Slice(0, separatorIndex + 1).ToArray();
        stream.Position = streamPositionBeforeRead + separatorIndex + 1;
        return lineCopy;
    }

    /// <summary>
    /// Reads the chunk.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="startPosition">The start position.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ReadOnlyMemory.</returns>
    public static ReadOnlyMemory<byte> ReadChunk(this Stream stream, long startPosition, int chunkSize, byte[]? buffer = null)
    {

        if (!stream.CanRead)
            throw new InvalidOperationException("Stream is not readable.");

        if (!stream.CanSeek)
            throw new InvalidOperationException("Stream does not support seeking.");

        buffer ??= new byte[chunkSize];
        if (buffer.Length < chunkSize)
            throw new ArgumentException("Buffer size must be at least chunkSize.", nameof(buffer));

        stream.Seek(startPosition, SeekOrigin.Begin);
        int bytesRead = stream.Read(buffer, 0, chunkSize);

        return new ReadOnlyMemory<byte>(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Writes the chunk.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="lines">The lines.</param>
    public static void WriteChunk(this Stream stream, IReadOnlyList<ByteChunkData> lines, int chunkSize, byte[]? buffer = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must support writing", nameof(stream));

        ArgumentNullException.ThrowIfNull(lines);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize, 0);

        buffer ??= new byte[chunkSize];
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length,chunkSize);

        int bufferOffset = 0;
        foreach (var line in lines)
        {
            if (bufferOffset + line.Length > buffer.Length)
            {
                stream.Write(buffer, 0, bufferOffset);
                bufferOffset = 0;
            }
            line.CopyToSpan(buffer.AsSpan(bufferOffset));
            bufferOffset += line.Length;
        }
        if (bufferOffset > 0)
        {
            stream.Write(buffer, 0, bufferOffset);
        }
    }

    /// <summary>
    /// Determines the chunk start positions.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="partsCount">The parts count.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>A list of long.</returns>
    public static IReadOnlyList<long> DetermineChunkStartPositions(this Stream stream, int partsCount, long chunkSize, byte separator)
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
                long correctedPosition = stream.IndexOf(startPosition, separator, searchBuffer);

                if (correctedPosition > 0)
                {
                    startPositions.Add(correctedPosition);
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
