using RowsSorter.Interfaces;
using RowsSorter.Shared;

namespace RowsSorter.Merger
{
    public class FoldingChunkMerger : IChunkMerger
    {
        private readonly IChunkMerger _chunkMerger;
        public FoldingChunkMerger(IChunkMerger chunkMerger)
        {
            _chunkMerger = chunkMerger;
        }


        public void MergeSortedChunks(TempFileCollection files)
        {
            var fileManager = new TemporaryFileManager(files);
            var counter = 0;
            while (!fileManager.IsEmpty && fileManager.Count > 2)
            {
                var batches = fileManager.GetNextMergeBatch(counter);
                var mergeTasks = new List<Task>();

                foreach (var item in batches)
                {
                    mergeTasks.Add(Task.Run(() =>
                    {
                        _chunkMerger.MergeSortedChunks(item);
                    }));
                }

                Task.WhenAll(mergeTasks).GetAwaiter().GetResult();

                counter++;
            }

            fileManager.Clear();
        }


        public async ValueTask MergeSortedChunksAsync(TempFileCollection files)
        {
            var fileManager = new TemporaryFileManager(files);
            var counter = 0;
            while (!fileManager.IsEmpty && fileManager.Count > 2)
            {
                var batches = fileManager.GetNextMergeBatch(counter);
                var mergeTasks = new List<Task>();

                foreach (var item in batches)
                {
                    mergeTasks.Add(Task.Run(async () =>
                    {
                        await _chunkMerger.MergeSortedChunksAsync(item);
                    }));
                }

                await Task.WhenAll(mergeTasks);

                counter++;
            }

            fileManager.Clear();
        }
    }
}
