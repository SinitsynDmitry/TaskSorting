using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Merger
{
    public class ChunkFoldingMerger : IChunkMerger
    {
        private readonly IChunkMerger _chunkMerger;
        public ChunkFoldingMerger(IChunkMerger chunkMerger)
        {
            _chunkMerger = chunkMerger;
        }


        /// <summary>
        /// Merges the sorted chunks.
        /// </summary>
        /// <param name="files">The files.</param>
        public void MergeSortedChunks(TempFileCollection files)
        {
            var fileManager = new TemporaryFileManager(files);
            var mergeTasks = new List<Task>((int)Math.Ceiling(files.Chunks.Count / (double)fileManager.MaxChunksPerTask));
            var counter = 0;
            while (!fileManager.IsEmpty && fileManager.Count >= 2)
            {
                var batches = fileManager.GetNextMergeBatch(counter);
               
                foreach (var item in batches)
                {
                    var proccesingFiles = item;
                    mergeTasks.Add(Task.Run(() =>
                    {
                        _chunkMerger.MergeSortedChunks(proccesingFiles);
                    }));
                }

                Task.WhenAll(mergeTasks).GetAwaiter().GetResult();
                mergeTasks.Clear();
                counter++;
            }

            fileManager.Clear();
        }


        /// <summary>
        /// Merges the sorted chunks async.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>A ValueTask.</returns>
        public async ValueTask MergeSortedChunksAsync(TempFileCollection files)
        {
            var fileManager = new TemporaryFileManager(files);
            var mergeTasks = new List<Task>((int)Math.Ceiling(files.Chunks.Count / (double)fileManager.MaxChunksPerTask));
            var counter = 0;
            while (!fileManager.IsEmpty && fileManager.Count > 2)
            {
                var batches = fileManager.GetNextMergeBatch(counter);

                foreach (var item in batches)
                {
                    mergeTasks.Add(Task.Run(async () =>
                    {
                        await _chunkMerger.MergeSortedChunksAsync(item);
                    }));
                }

                await Task.WhenAll(mergeTasks);
                mergeTasks.Clear();
                counter++;
            }

            fileManager.Clear();
        }
    }
}
