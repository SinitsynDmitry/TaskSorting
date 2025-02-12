using System.Buffers;

namespace RowsSorter.BinaryBlockExt;

public readonly struct BinaryBlock
{
    /// <summary>
    /// Gets the repeat count.
    /// </summary>
    public readonly long RepeatCount { get; }
    /// <summary>
    /// Gets the data.
    /// </summary>
    public readonly ReadOnlyMemory<byte> Data { get; }
    private readonly bool IsPooled;
    private readonly ArrayPool<byte> Pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryBlock"/> class.
    /// </summary>
    /// <param name="repeatCount">The repeat count.</param>
    /// <param name="data">The data.</param>
    /// <param name="isPooled">If true, is pooled.</param>
    /// <param name="pool">The pool.</param>
    public BinaryBlock(long repeatCount, ReadOnlyMemory<byte> data, bool isPooled = false, ArrayPool<byte> pool = null)
    {
        if (repeatCount <= 0)
            throw new ArgumentException("RepeatCount must be positive", nameof(repeatCount));

        RepeatCount = repeatCount;
        Data = data;
        IsPooled = isPooled;
        Pool = pool;
    }

    /// <summary>
    /// Writes the decompressed.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void WriteDecompressed(Stream stream)
    {
        for (long i = 0; i < RepeatCount; i++)
        {
            stream.Write(Data.Span);
        }
    }

    // Освобождаем память, если блок использует пул
    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        if (IsPooled && Data.Length > 0)
        {
            Pool?.Return(Data.ToArray());
        }
    }
}

public class BufferedBinaryReader : IDisposable
{
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly byte[] _buffer;
    private readonly ArrayPool<byte> _pool;
    private int _bufferPosition;
    private int _bufferCount;
    private const int DefaultBufferSize = 81920; // 80KB

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedBinaryReader"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="bufferSize">The buffer size.</param>
    public BufferedBinaryReader(Stream stream, int bufferSize = DefaultBufferSize)
    {
        _stream = stream;
        _reader = new BinaryReader(stream, encoding: null);
        _pool = ArrayPool<byte>.Shared;
        _buffer = _pool.Rent(bufferSize);
        _bufferPosition = 0;
        _bufferCount = 0;
    }

    /// <summary>
    /// Disposes the.
    /// </summary>
    public void Dispose()
    {
        _pool.Return(_buffer);
        _reader.Dispose();
    }

    /// <summary>
    /// Fills the buffer.
    /// </summary>
    private void FillBuffer()
    {
        _bufferCount = _stream.Read(_buffer, 0, _buffer.Length);
        _bufferPosition = 0;
    }

    /// <summary>
    /// Reads the int64.
    /// </summary>
    /// <returns>A long.</returns>
    public long ReadInt64()
    {
        EnsureBuffer(8);
        long result = BitConverter.ToInt64(_buffer, _bufferPosition);
        _bufferPosition += 8;
        return result;
    }

    /// <summary>
    /// Reads the int32.
    /// </summary>
    /// <returns>An int.</returns>
    public int ReadInt32()
    {
        EnsureBuffer(4);
        int result = BitConverter.ToInt32(_buffer, _bufferPosition);
        _bufferPosition += 4;
        return result;
    }

    /// <summary>
    /// Ensures the buffer.
    /// </summary>
    /// <param name="needed">The needed.</param>
    private void EnsureBuffer(int needed)
    {
        if (_bufferPosition + needed > _bufferCount)
        {
            // Сдвигаем оставшиеся данные в начало буфера
            if (_bufferCount > _bufferPosition)
            {
                Buffer.BlockCopy(_buffer, _bufferPosition, _buffer, 0, _bufferCount - _bufferPosition);
                _bufferCount -= _bufferPosition;
            }
            else
            {
                _bufferCount = 0;
            }
            _bufferPosition = 0;

            // Дочитываем буфер
            int read = _stream.Read(_buffer, _bufferCount, _buffer.Length - _bufferCount);
            _bufferCount += read;

            if (_bufferCount < needed)
                throw new EndOfStreamException();
        }
    }

    /// <summary>
    /// Reads the bytes.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns>A ReadOnlyMemory.</returns>
    public ReadOnlyMemory<byte> ReadBytes(int length)
    {
        byte[] result = _pool.Rent(length);
        int totalRead = 0;

        while (totalRead < length)
        {
            int remaining = length - totalRead;
            int availableInBuffer = _bufferCount - _bufferPosition;

            if (availableInBuffer == 0)
            {
                FillBuffer();
                availableInBuffer = _bufferCount;
                if (availableInBuffer == 0)
                    break; // EOF
            }

            int toCopy = Math.Min(remaining, availableInBuffer);
            Buffer.BlockCopy(_buffer, _bufferPosition, result, totalRead, toCopy);

            _bufferPosition += toCopy;
            totalRead += toCopy;
        }

        if (totalRead != length)
        {
            _pool.Return(result);
            throw new EndOfStreamException();
        }

        return new ReadOnlyMemory<byte>(result, 0, length);
    }
}

public class BinarySerializer
{
    private const int BufferSize = 81920; // 80KB

    /// <summary>
    /// Serializes the.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="blocks">The blocks.</param>
    public void Serialize(Stream stream, IEnumerable<BinaryBlock> blocks)
    {
        using (var writer = new BinaryWriter(stream, encoding: null, leaveOpen: true))
        {
            foreach (var block in blocks)
            {
                writer.Write(block.RepeatCount);
                writer.Write(block.Data.Length);
                writer.Write(block.Data.Span);
            }
        }
    }

    /// <summary>
    /// Deserializes the.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>A list of BinaryBlocks.</returns>
    public List<BinaryBlock> Deserialize(Stream stream)
    {
        var result = new List<BinaryBlock>();

        using (var reader = new BufferedBinaryReader(stream))
        {
            while (stream.Position < stream.Length)
            {
                long repeatCount = reader.ReadInt64();
                int length = reader.ReadInt32();
                var data = reader.ReadBytes(length);
                result.Add(new BinaryBlock(repeatCount, data, isPooled: true, pool: ArrayPool<byte>.Shared));
            }
        }

        return result;
    }
}
