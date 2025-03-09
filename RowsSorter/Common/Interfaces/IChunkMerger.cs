using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;

namespace RowsSorter.Common.Interfaces;

/// <summary>
/// Handles merging of pre-sorted chunks
/// </summary>
public interface IChunkMerger
{
    /// <summary>
    /// Merges the sorted chunks.
    /// </summary>
    /// <param name="files">The files.</param>
    void MergeSortedChunks(TempFileCollection files);

    /// <summary>
    /// Merges the sorted chunks async.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask MergeSortedChunksAsync(TempFileCollection files);
}
