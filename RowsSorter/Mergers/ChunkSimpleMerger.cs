using RowsSorter.Entities;
using RowsSorter.Interfaces;
using RowsSorter.Pipeline;
using RowsSorter.Pipeline.Contexts;
using RowsSorter.Pipeline.Steps;
using RowsSorter.Shared;

namespace RowsSorter.Merger;

public class ChunkSimpleMerger : IChunkMerger
{
    private const int MEMORY_BUFFER_SIZE = 65536;
    private readonly IComparer<ByteChunkData> _comparer;
    private readonly IPipeline<MergingPipelineContext> _mergerPipeline;
    private readonly IStreamProvider _streamProvider;

    public ChunkSimpleMerger(
        IComparer<ByteChunkData> comparer,
        IStreamProvider streamProvider,
        IPipeline<MergingPipelineContext>? customPipeline = null
    )
    {
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(streamProvider);

        _comparer = comparer;
        _streamProvider = streamProvider;
        _mergerPipeline = customPipeline ?? CreateDefaultPipeline();
    }

    /// <summary>
    /// Merges the sorted chunks.
    /// </summary>
    /// <param name="files">The files.</param>
    public void MergeSortedChunks(TempFileCollection files)
    {
        try
        {
            using (var merger = CreateMergerResources(files))
            {
                _mergerPipeline.Execute(merger.Context);
                merger.Writer.Flush();
            }
        }
        finally
        {
            files.DeleteFiles();
        }
    }

    /// <summary>
    /// Merges the sorted chunks async.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask MergeSortedChunksAsync(TempFileCollection files)
    {
        try
        {
            using (var merger = CreateMergerResources(files))
            {
                await _mergerPipeline.ExecuteAsync(merger.Context);
                await merger.Writer.FlushAsync();
            }
        }
        finally
        {
            files.DeleteFiles();
        }
    }

    /// <summary>
    /// Creates the default pipeline.
    /// </summary>
    /// <returns>An IPipeline.</returns>
    private static IPipeline<MergingPipelineContext> CreateDefaultPipeline() =>
        new ProcessingPipeline<MergingPipelineContext>()
            .AddStep(new InitializeQueueStep())
            .AddStep(new ProcessQueueStep());

    /// <summary>
    /// Creates the merger resources.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>A MergerResources.</returns>
    private MergerResources CreateMergerResources(TempFileCollection files)
    {
        var readers = new StreamCollection(files.Chunks, _streamProvider);
        var sortingQueue = new StreamMergeQueue(_comparer);
        var writer = new BufferedStream(
            _streamProvider.GetWriteStream(files.OutputFile),
            MEMORY_BUFFER_SIZE
        );

        return new MergerResources(
            new MergingPipelineContext(readers, sortingQueue, writer),
            writer
        );
    }

    private sealed class MergerResources : IDisposable
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        public MergingPipelineContext Context { get; }

        /// <summary>
        /// Gets the writer.
        /// </summary>
        public BufferedStream Writer { get; }

        public MergerResources(MergingPipelineContext context, BufferedStream writer)
        {
            Context = context;
            Writer = writer;
        }

        /// <summary>
        /// Disposes the.
        /// </summary>
        public void Dispose()
        {
            Context.Dispose();
            Writer.Dispose();
        }
    }
}
