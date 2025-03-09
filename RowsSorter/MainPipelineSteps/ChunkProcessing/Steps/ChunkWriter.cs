using RowsSorter.Common.Interfaces;
using RowsSorter.MainPipelineSteps.ChunkProcessing.Context;

namespace RowsSorter.MainPipelineSteps.ChunkProcessing.Steps;
public class ChunkWriter : IChunkWriter
{
    private readonly IStreamProvider _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkWriter"/> class.
    /// </summary>
    public ChunkWriter(IStreamProvider streamProvider)
    {
        ArgumentNullException.ThrowIfNull(streamProvider);
        _streamProvider = streamProvider;
    }

    /// <summary>
    /// Writes the chunk.
    /// </summary>
    /// <param name="context">The context.</param>
    public void WriteChunk(ChunkProcessingContext context)
    {
        using var fileStream = _streamProvider.GetWriteStream(context.OutputFile, context.BytesRead);

        fileStream.Write(context.Buffer, 0, context.BytesRead);
    }

    /// <summary>
    /// Writes the chunk async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask WriteChunkAsync(ChunkProcessingContext context)
    {
        using var fileStream = _streamProvider.GetWriteStream(context.OutputFile, context.BytesRead, true);

        await fileStream.WriteAsync(context.Buffer, 0, context.BytesRead);
    }
}
