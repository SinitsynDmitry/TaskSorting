namespace RowsSorter.Interfaces;

/// <summary>
/// Handles merging of pre-sorted chunks
/// </summary>
public interface IChunkMerger
{
    void MergeSortedChunks(IReadOnlyList<string> chunkFiles, string outputFile);
    Task MergeSortedChunksAsync(IReadOnlyList<string> chunkFiles, string outputFile);
}
