using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

namespace FileCreator;

/// <summary>
/// The file generator.
/// </summary>
public class FileGenerator
{

    private readonly ImmutableArray<string> _vocabulary;
    const int BUFFER_SIZE = 64 * 1024;
    const int NUMBER_PART_MAX = 5000;

    private readonly int _partitionSize;
    private readonly int _avgRowSize;
    private readonly object _lockObject = new();

    private static readonly ThreadLocal<Random> _localRandom = new ThreadLocal<Random>(() => new Random());
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileGenerator"/> class.
    /// </summary>
    /// <param name="vocabulary">The vocabulary.</param>
    /// <param name="partitionSize">The partition size.</param>
    /// <param name="avgRowSize">The avg row size.</param>
    public FileGenerator(string[] vocabulary, int partitionSize = 1000000, int avgRowSize = 50)
    {
        _vocabulary = ImmutableArray.Create(vocabulary);
        _partitionSize = partitionSize;
        _avgRowSize = avgRowSize;
        _stringBuilderPool = new DefaultObjectPoolProvider()
            .CreateStringBuilderPool(_partitionSize * _avgRowSize, _partitionSize * _avgRowSize * 2);
    }

    /// <summary>
    /// Generates the file async. Lower speed means less memory.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    /// <returns>A Task.</returns>
    public async Task GenerateFileAsync(string filePath, long rowsInFile)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        using var writer = new StreamWriter(filePath, true, Encoding.UTF8, bufferSize: BUFFER_SIZE);
        writer.AutoFlush = false;

        var rangePartitioner = Partitioner.Create(0L, rowsInFile, _partitionSize).AsParallel();
        var localStringBuffer = new StringBuilder(_partitionSize * _avgRowSize);
        var random = _localRandom.Value;

        foreach (var range in rangePartitioner)
        {
            for (long i = range.Item1; i < range.Item2; i++)
            {
                AppendOneRow(random, localStringBuffer);
            }

            if (localStringBuffer.Length > 0)
            {
                foreach (var chunk in localStringBuffer.GetChunks())
                {
                    await writer.WriteAsync(chunk);
                }
                await writer.FlushAsync();
                localStringBuffer.Clear();
            }
        }
    }

    /// <summary>
    /// Generates the file. Lower speed means less memory.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    public void GenerateFile(string filePath, long rowsInFile)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        using var writer = new StreamWriter(filePath, true, Encoding.UTF8, bufferSize: BUFFER_SIZE);
        writer.AutoFlush = false;

        var rangePartitioner = Partitioner.Create(0L, rowsInFile, _partitionSize).AsParallel();
        var localStringBuffer = new StringBuilder(_partitionSize * _avgRowSize);
        var random = _localRandom.Value;

        foreach (var range in rangePartitioner)
        {
            for (long i = range.Item1; i < range.Item2; i++)
            {
                AppendOneRow(random, localStringBuffer);
            }

            if (localStringBuffer.Length > 0)
            {
                foreach (var chunk in localStringBuffer.GetChunks())
                {
                    writer.Write(chunk);
                }
                writer.Flush();
                localStringBuffer.Clear();
            }
        }
    }

    /// <summary>
    /// Generates the file as parallel. More speed - more memory.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    public void GenerateFileAsParallel(string filePath, long rowsInFile)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        using var writer = new StreamWriter(filePath, true, Encoding.UTF8, bufferSize: BUFFER_SIZE);
        writer.AutoFlush = false;

        var rangePartitioner = Partitioner.Create(0L, rowsInFile, _partitionSize).AsParallel();

        Parallel.ForEach(rangePartitioner, range =>
        {
            RangeHandler(range.Item1, range.Item2, writer);
        });
    }


    /// <summary>
    /// Generates the file as parallel async. More speed - more memory.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    /// <returns>A Task.</returns>
    public async Task GenerateFileAsParallelAsync(string filePath, long rowsInFile)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        using var writer = new StreamWriter(filePath, true, Encoding.UTF8, bufferSize: BUFFER_SIZE);
        writer.AutoFlush = false;

        var rangePartitioner = Partitioner.Create(0L, rowsInFile, _partitionSize).AsParallel();

        await Parallel.ForEachAsync(rangePartitioner, async (range, _) =>
        {
            await RangeHandlerAsync(range.Item1, range.Item2, writer);
        });
    }

    /// <summary>
    /// Ranges the handler.
    /// </summary>
    /// <param name="rangeMin">The range min.</param>
    /// <param name="rangeMax">The range max.</param>
    /// <param name="writer">The writer.</param>
    private void RangeHandler(long rangeMin, long rangeMax, StreamWriter writer)
    {
        StringBuilder localStringBuffer = _stringBuilderPool.Get();
        try
        {
            var random = _localRandom.Value;

            for (long i = rangeMin; i < rangeMax; i++)
            {
                AppendOneRow(random, localStringBuffer);
            }

            lock (_lockObject)
            {
                foreach (var chunk in localStringBuffer.GetChunks())
                {
                    writer.Write(chunk);
                }
                writer.Flush();
            }
        }
        finally
        {
            localStringBuffer.Clear();
            _stringBuilderPool.Return(localStringBuffer);
        }
    }

    /// <summary>
    /// Ranges the handler async.
    /// </summary>
    /// <param name="rangeMin">The range min.</param>
    /// <param name="rangeMax">The range max.</param>
    /// <param name="writer">The writer.</param>
    /// <returns>A Task.</returns>
    private async Task RangeHandlerAsync(long rangeMin, long rangeMax, StreamWriter writer)
    {
        StringBuilder localStringBuffer = _stringBuilderPool.Get();
        try
        {
            var random = _localRandom.Value;

            for (long i = rangeMin; i < rangeMax; i++)
            {
                AppendOneRow(random, localStringBuffer);
            }

            lock (_lockObject)
            {
                foreach (var chunk in localStringBuffer.GetChunks())
                {
                    writer.Write(chunk);
                }
                writer.Flush();
            }

            await Task.CompletedTask;
        }
        finally
        {
            localStringBuffer.Clear();
            _stringBuilderPool.Return(localStringBuffer);
        }
    }

    /// <summary>
    /// Appends the one row to buffer.
    /// </summary>
    /// <param name="random">The random.</param>
    /// <param name="localStringBuffer">The local string buffer.</param>
    private void AppendOneRow(Random? random, StringBuilder localStringBuffer)
    {
        int rowInt = random.Next(1, NUMBER_PART_MAX);
        int vocabularyIndex = random.Next(0, _vocabulary.Length);
        localStringBuffer.Append(rowInt).Append(_vocabulary[vocabularyIndex]);
    }

}
