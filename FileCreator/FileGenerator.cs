using System;
using System.Runtime.CompilerServices;
using System.Text;
using FileCreator.Interfaces;

namespace FileCreator;

/// <summary>
/// The file generator.
/// </summary>
public class FileGenerator: IAsyncFileGenerator, IFileGenerator
{
    private readonly IContentStrategy _contentStrategy;
    private readonly IGeneratorConfig _config;

    public FileGenerator(IContentStrategy contentStrategy, IGeneratorConfig config)
    {
        _contentStrategy = contentStrategy;
        _config = config;
    }


    /// <summary>
    /// Generates the file.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    public void GenerateFile(IContentWriter writer, long rowsInFile)
    {
        var localStringBuffer = new StringBuilder(
            capacity: Math.Min(_config.PartitionSize * _config.EstimatedMaxRowSize, Array.MaxLength)
        );

        for (long i = 0; i < rowsInFile; i += _config.PartitionSize)
        {
            var end = Math.Min(i + _config.PartitionSize, rowsInFile);

            GeneratePartition(localStringBuffer, i, end);

            writer.WriteBuffer(localStringBuffer);
        }

        //writer.Flush();
    }


    /// <summary>
    /// Generates the file async.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="rowsInFile">The rows in file.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask GenerateFileAsync(IContentWriter writer, long rowsInFile)
    {

        var localStringBuffer = new StringBuilder(
            capacity: Math.Min(_config.PartitionSize * _config.EstimatedMaxRowSize, Array.MaxLength)
        );

        for (long i = 0; i < rowsInFile; i += _config.PartitionSize)
        {
            var end = Math.Min(i + _config.PartitionSize, rowsInFile);

            GeneratePartition(localStringBuffer, i, end);

            await writer.WriteBufferAsync(localStringBuffer);
        }

       // await writer.FlushAsync();
    }

    /// <summary>
    /// Writes the buffer async.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="writer">The writer.</param>
    /// <returns>A ValueTask.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask WriteBufferAsync(StringBuilder buffer, StreamWriter writer)
    {
        if (buffer.Length > 0)
        {
            foreach (var chunk in buffer.GetChunks())
            {
                await writer.WriteAsync(chunk);
            }

            buffer.Clear();
        }
    }

    /// <summary>
    /// Generates the partition.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="writer">The _writer.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GeneratePartition(
        StringBuilder buffer,
        long start,
        long end
        )
    {
        for (long j = start; j < end; j++)
        {
            _contentStrategy.FillBuffer(buffer);
        }
    }

    /// <summary>
    /// Writes the buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="writer">The writer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBuffer(StringBuilder buffer, StreamWriter writer)
    {
        if (buffer.Length > 0)
        {
            foreach (var chunk in buffer.GetChunks())
            {
                writer.Write(chunk);
            }

            buffer.Clear();
        }
    }

}
