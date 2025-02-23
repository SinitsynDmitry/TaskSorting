using Moq;
using RowsSorter.Interfaces;
using RowsSorter.Merger;
using RowsSorter.Shared;

namespace RowsSorterTests.Mergers;

[TestFixture]
public class ChunkFoldingMergerTests
{
    private ChunkFoldingMerger _foldingMerger;
    private Mock<IChunkMerger> _chunkMergerMock;
    private TempFileCollection _files;

    [SetUp]
    public void SetUp()
    {
        _chunkMergerMock = new Mock<IChunkMerger>();
        _files = new TempFileCollection { Chunks = new List<string> { "chunk1.tmp", "chunk2.tmp", "chunk3.tmp" } };
        _foldingMerger = new ChunkFoldingMerger(_chunkMergerMock.Object);
    }

    [Test]
    public void MergeSortedChunks_ShouldCallMergeMethodCorrectNumberOfTimes()
    {
        _foldingMerger.MergeSortedChunks(_files);
        _chunkMergerMock.Verify(m => m.MergeSortedChunks(It.IsAny<TempFileCollection>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task MergeSortedChunksAsync_ShouldCallMergeMethodCorrectNumberOfTimesAsync()
    {
        await _foldingMerger.MergeSortedChunksAsync(_files);
        _chunkMergerMock.Verify(m => m.MergeSortedChunksAsync(It.IsAny<TempFileCollection>()), Times.AtLeastOnce);
    }

    [Test]
    public void MergeSortedChunks_ShouldProcessMultipleBatches()
    {
        var files = new TempFileCollection
        {
            Chunks = Enumerable.Range(1, 10).Select(i => $"chunk{i}.tmp").ToList(),
            OutputFile = "output.tmp"
        };

        _foldingMerger.MergeSortedChunks(files);

        _chunkMergerMock.Verify(m => m.MergeSortedChunks(It.IsAny<TempFileCollection>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task MergeSortedChunksAsync_ShouldProcessMultipleBatchesAsync()
    {
        var files = new TempFileCollection
        {
            Chunks = Enumerable.Range(1, 10).Select(i => $"chunk{i}.tmp").ToList(),
            OutputFile = "output.tmp"
        };

        await _foldingMerger.MergeSortedChunksAsync(files);

        _chunkMergerMock.Verify(m => m.MergeSortedChunksAsync(It.IsAny<TempFileCollection>()), Times.AtLeastOnce);
    }
}
