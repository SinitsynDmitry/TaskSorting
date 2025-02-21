using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Splitter;

public class ChunkReader : IChunkReader
{
    private readonly IStreamProvider _streamProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkReader"/> class.
    /// </summary>
    public ChunkReader(IStreamProvider streamProvider)
    {
        ArgumentNullException.ThrowIfNull(streamProvider);
        _streamProvider = streamProvider;
    }

    /// <summary>
    /// Reads the chunk.
    /// </summary>
    /// <param name="context">The context.</param>
    public void ReadChunk(ChunkProcessingContext context)
    {
        using var fileStream = _streamProvider.GetReadStream(context.InputFile, context.Buffer.Length);
        fileStream.Seek(context.StartPosition, SeekOrigin.Begin);
        context.BytesRead = fileStream.Read(context.Buffer, 0, context.ChunkSize);
    }

    /// <summary>
    /// Reads the chunk async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ReadChunkAsync(ChunkProcessingContext context)
    {
        using var fileStream = _streamProvider.GetReadStream(context.InputFile, context.ChunkSize, true);

        fileStream.Seek(context.StartPosition, SeekOrigin.Begin);
        context.BytesRead = await fileStream.ReadAsync(context.Buffer, 0, context.ChunkSize);
    }
}
