using System.Buffers;
using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Pipeline.Steps;

public class LineSorterProcessingStep : IProcessingStep<ChunkProcessingContext>
{
    private readonly ILineSorter _lineSorter;

    public LineSorterProcessingStep(ILineSorter lineSorter)
    {
        _lineSorter = lineSorter;
    }

    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Process(ChunkProcessingContext context)
    {
        _lineSorter.Sort(context);
    }

    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ProcessAsync(ChunkProcessingContext context)
    {
        _lineSorter.Sort(context);
    }
}

public class ChunkReaderProcessingStep : IProcessingStep<ChunkProcessingContext>
{
    private readonly IChunkReader _chunkReader;

    public ChunkReaderProcessingStep(IChunkReader chunkReader)
    {
        _chunkReader = chunkReader;
    }

    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Process(ChunkProcessingContext context)
    {
        _chunkReader.ReadChunk(context);
    }

    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ProcessAsync(ChunkProcessingContext context)
    {
        await _chunkReader.ReadChunkAsync(context);
    }
}

public class ChunkProcessingStep : IProcessingStep<FileProcessingContext>
{

    private readonly IPipeline<ChunkProcessingContext> _chunkPipeline;

    public ChunkProcessingStep(IChunkReader chunkReader, ILineSorter lineSorter, IChunkWriter chunkWriter)
    {
        _chunkPipeline = new ProcessingPipeline<ChunkProcessingContext>()
            .AddStep(new ProcessingStep<ChunkProcessingContext>(
                syncProcess: context => chunkReader.ReadChunk(context),
                asyncProcess: context => chunkReader.ReadChunkAsync(context)
            )).AddStep(ProcessingStep<ChunkProcessingContext>.FromSyncOnly<ChunkProcessingContext>(
                context => lineSorter.Sort(context)
            )
            ).AddStep(new ProcessingStep<ChunkProcessingContext>(
                syncProcess: context => chunkWriter.WriteChunk(context),
                asyncProcess: context => chunkWriter.WriteChunkAsync(context)
            ));
    }

    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ProcessAsync(FileProcessingContext context)
    {
        var tasks = new ValueTask[context.TempChunks.Count];

        for (int i = 0; i < context.TempChunks.Count; i++)
        {
            var chunkContext = ChunkProcessingContext.Create(
                context.InputFile,
                context.TempChunks[i].outputFile,
                context.TempChunks[i].startPosition,
                context.TempChunks[i].chunkSize
            );
            tasks[i] = ProcessChunkAsync(chunkContext);
        }

        await Task.WhenAll(tasks.Select(t => t.AsTask()));
    }
    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Process(FileProcessingContext context)
    {
        ProcessAsync(context).AsTask().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Processes the chunk async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    private async ValueTask ProcessChunkAsync(ChunkProcessingContext context)
    {
        try
        {
            await _chunkPipeline.ExecuteAsync(context);
        }
        finally
        {
            context.ReleaseBuffer();
        }
    }
}
