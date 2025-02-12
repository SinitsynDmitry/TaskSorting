namespace RowsSorter.Interfaces;

/// <summary>
/// Handles merging of pre-sorted chunks
/// </summary>
public interface IChunkMerger
{
    /// <summary>
    /// Merges the sorted chunks.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    void MergeSortedChunks(IReadOnlyList<string> chunkFiles, string outputFile);
    /// <summary>
    /// Merges the sorted chunks async.
    /// </summary>
    /// <param name="chunkFiles">The chunk files.</param>
    /// <param name="outputFile">The output file.</param>
    /// <returns>A Task.</returns>
    ValueTask MergeSortedChunksAsync(IReadOnlyList<string> chunkFiles, string outputFile);
}
