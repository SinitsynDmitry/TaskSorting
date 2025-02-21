using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;
using RowsSorter.Shared;

namespace RowsSorter.Pipeline.Steps
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
            var files = new TempFileCollection
            {
                Chunks = context.TempChunks.Select(f => f.outputFile).ToList(),
                OutputFile = context.OutputFile
            };

            _chunkMerger.MergeSortedChunks(files);
        }

        /// <summary>
        /// Processes the async.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A ValueTask.</returns>
        public async ValueTask ProcessAsync(FileProcessingContext context)
        {
            var files = new TempFileCollection
            {
                Chunks = context.TempChunks.Select(f => f.outputFile).ToList(),
                OutputFile = context.OutputFile
            };

            await _chunkMerger.MergeSortedChunksAsync(files);
        }
    }
}
