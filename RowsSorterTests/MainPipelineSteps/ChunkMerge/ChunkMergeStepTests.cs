using Moq;
using RowsSorter.Common.Interfaces;
using RowsSorter.MainPipelineSteps.ChunkMerge;
using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;
using RowsSorter.MainPipelineSteps.Context;

namespace RowsSorterTests.MainPipelineSteps.ChunkMerge;

[TestFixture]
public class ChunkMergeStepTests
{
    private Mock<IChunkMerger> _chunkMergerMock;
    private ChunkMergeStep _chunkMergeStep;

    [SetUp]
    public void Setup()
    {
        _chunkMergerMock = new Mock<IChunkMerger>();
        _chunkMergeStep = new ChunkMergeStep(_chunkMergerMock.Object);
    }

    [Test]
    public void Process_ShouldCallMergeSortedChunks_WhenCalledWithValidContext()
    {
        var context = new FileProcessingContext(
            "input.txt", "output.txt", 1024, "tempDir"
        );
        context.TempChunks.Add((0, 1024, "tempChunk1"));
        context.TempChunks.Add((1024, 1024, "tempChunk2"));

        var expectedFiles = new TempFileCollection
        {
            Chunks = context.TempChunks.Select(f => f.outputFile).ToArray(),
            OutputFile = context.OutputFile
        };

        _chunkMergeStep.Process(context);

        _chunkMergerMock.Verify(m => m.MergeSortedChunks(It.Is<TempFileCollection>(t =>
            t.Chunks.SequenceEqual(expectedFiles.Chunks) &&
            t.OutputFile == expectedFiles.OutputFile
        )), Times.Once);
    }

    [Test]
    public async Task ProcessAsync_ShouldCallMergeSortedChunksAsync_WhenCalledWithValidContext()
    {
        var context = new FileProcessingContext(
            "input.txt", "output.txt", 1024, "tempDir"
        );
        context.TempChunks.Add((0, 1024, "tempChunk1"));
        context.TempChunks.Add((1024, 1024, "tempChunk2"));

        var expectedFiles = new TempFileCollection
        {
            Chunks = context.TempChunks.Select(f => f.outputFile).ToArray(),
            OutputFile = context.OutputFile
        };

        await _chunkMergeStep.ProcessAsync(context);

        _chunkMergerMock.Verify(m => m.MergeSortedChunksAsync(It.Is<TempFileCollection>(t =>
            t.Chunks.SequenceEqual(expectedFiles.Chunks) &&
            t.OutputFile == expectedFiles.OutputFile
        )), Times.Once);
    }

    [Test]
    public void Process_ShouldNotCallMergeSortedChunks_WhenTempChunksIsEmpty()
    {

        var context = new FileProcessingContext(
            "input.txt", "output.txt", 1024, "tempDir"
        );

        _chunkMergeStep.Process(context);

        _chunkMergerMock.Verify(m => m.MergeSortedChunks(It.IsAny<TempFileCollection>()), Times.Never);
    }

    [Test]
    public async Task ProcessAsync_ShouldNotCallMergeSortedChunksAsync_WhenTempChunksIsEmpty()
    {
        var context = new FileProcessingContext(
            "input.txt", "output.txt", 1024, "tempDir"
        );

        await _chunkMergeStep.ProcessAsync(context);

        _chunkMergerMock.Verify(m => m.MergeSortedChunksAsync(It.IsAny<TempFileCollection>()), Times.Never);
    }
}
