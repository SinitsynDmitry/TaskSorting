using RowsSorter.Common.Interfaces;
using RowsSorter.Common.Pipeline;
using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;
using RowsSorter.MainPipelineSteps.Context;
using RowsSorter.Shared;

namespace RowsSorter.MainPipelineSteps.ChunkMerge
{
    public class ChunkMergeStep : IProcessingStep<FileProcessingContext>
    {
        private readonly IChunkMerger _chunkMerger;

        public ChunkMergeStep(IChunkMerger chunkMerger)
        {
            _chunkMerger = chunkMerger;
        }

        /// <summary>
        /// Processes the.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Process(FileProcessingContext context)
        {
            if (context.TempChunks.Count > 0)
            {
                var files = new TempFileCollection
                {
                    Chunks = new ArraySegment<string>(context.TempChunks.Select(f => f.outputFile).ToArray()),
                    OutputFile = context.OutputFile,
                };

                _chunkMerger.MergeSortedChunks(files);
            }
        }

        /// <summary>
        /// Processes the async.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A ValueTask.</returns>
        public async ValueTask ProcessAsync(FileProcessingContext context)
        {
            if (context.TempChunks.Count > 0)
            {
                var files = new TempFileCollection
                {
                    Chunks = new ArraySegment<string>(context.TempChunks.Select(f => f.outputFile).ToArray()),
                    OutputFile = context.OutputFile,
                };

                await _chunkMerger.MergeSortedChunksAsync(files);
            }
        }
    }
}
