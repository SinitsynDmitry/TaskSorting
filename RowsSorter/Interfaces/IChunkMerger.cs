using RowsSorter.Shared;

namespace RowsSorter.Interfaces;

/// <summary>
/// Handles merging of pre-sorted chunks
/// </summary>
public interface IChunkMerger
{
    void MergeSortedChunks(TempFileCollection files);

    ValueTask MergeSortedChunksAsync(TempFileCollection files);
}
