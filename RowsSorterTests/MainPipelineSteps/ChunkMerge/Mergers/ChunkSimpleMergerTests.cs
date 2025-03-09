using System.Text;
using Moq;
using RowsSorter.Common.Interfaces;
using RowsSorter.Common.Pipeline;
using RowsSorter.Entities;
using RowsSorter.MainPipelineSteps.ChunkMerge.Context;
using RowsSorter.MainPipelineSteps.ChunkMerge.Mergers;
using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;

namespace RowsSorterTests.MainPipelineSteps.ChunkMerge.Mergers;

[TestFixture]
public class ChunkSimpleMergerTests
{
    private ChunkSimpleMerger _merger;
    private Mock<IComparer<ByteChunkData>> _comparerMock;
    private Mock<IStreamProvider> _streamProviderMock;
    private Mock<IPipeline<MergingPipelineContext>> _pipelineMock;
    private TempFileCollection _files;
    private MemoryStream _outputStream;
    private MemoryStream _reader1Stream;
    private MemoryStream _reader2Stream;

    [SetUp]
    public void SetUp()
    {
        _comparerMock = new Mock<IComparer<ByteChunkData>>();
        _streamProviderMock = new Mock<IStreamProvider>();
        _pipelineMock = new Mock<IPipeline<MergingPipelineContext>>();

        _files = new TempFileCollection
        {
            Chunks = new ArraySegment<string>(["chunk1.tmp", "chunk2.tmp"]),
            OutputFile = "output.tmp"
        };

        _outputStream = new MemoryStream();

        string chunk1 = "Apple\nBanana\nCherry\n";
        string chunk2 = "Apricot\nBlueberry\n";
        byte[] chunk1Bytes = Encoding.UTF8.GetBytes(chunk1);
        byte[] chunk2Bytes = Encoding.UTF8.GetBytes(chunk2);

        _reader1Stream = new MemoryStream(chunk1Bytes);
        _reader2Stream = new MemoryStream(chunk2Bytes);

        _streamProviderMock.Setup(sp => sp.GetReadStream("chunk1.tmp", It.IsAny<int>(), It.IsAny<bool>())).Returns(_reader1Stream);
        _streamProviderMock.Setup(sp => sp.GetReadStream("chunk2.tmp", It.IsAny<int>(), It.IsAny<bool>())).Returns(_reader2Stream);
        _streamProviderMock.Setup(sp => sp.GetWriteStream(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_outputStream);

        _merger = new ChunkSimpleMerger(_comparerMock.Object, _streamProviderMock.Object, null);
    }

    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        Assert.That(_merger, Is.Not.Null);
    }

    [Test]
    public void MergeSortedChunks_ShouldExecutePipeline()
    {
        _merger = new ChunkSimpleMerger(_comparerMock.Object, _streamProviderMock.Object, _pipelineMock.Object);
        _merger.MergeSortedChunks(_files);
       
        _pipelineMock.Verify(p => p.Execute(It.IsAny<MergingPipelineContext>()), Times.Once);
    }

    [Test]
    public void MergeSortedChunks_ShouldWriteOutput()
    {
        _merger.MergeSortedChunks(_files);
        long length;
        using (var tempStream = new MemoryStream(_outputStream.ToArray()))
        {
            length = tempStream.Length;
        }

        Assert.That(length, Is.GreaterThan(0), "Output stream should contain merged data");
    }


    [Test]
    public async Task MergeSortedChunksAsync_ShouldExecutePipeline()
    {
        _merger = new ChunkSimpleMerger(_comparerMock.Object, _streamProviderMock.Object, _pipelineMock.Object);
        await _merger.MergeSortedChunksAsync(_files);
        _pipelineMock.Verify(p => p.ExecuteAsync(It.IsAny<MergingPipelineContext>()), Times.Once);
    }

    [Test]
    public async Task MergeSortedChunksAsync_ShouldWriteOutput()
    {
        await _merger.MergeSortedChunksAsync(_files);
        long length;
        using (var tempStream = new MemoryStream(_outputStream.ToArray()))
        {
            length = tempStream.Length;
        }

        Assert.That(length, Is.GreaterThan(0), "Output stream should contain merged data");
    }

    [Test]
    public void MergeSortedChunks_ShouldPreserveTotalLineCount()
    {
        _merger.MergeSortedChunks(_files);
        long length;
        using (var tempStream = new MemoryStream(_outputStream.ToArray()))
        using (var reader = new StreamReader(tempStream, Encoding.UTF8))
        {
            string[] resultLines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
            length = resultLines.Length;
        }

        Assert.That(length, Is.EqualTo(5), "Merged file should contain correct number of lines");
    }

    [Test]
    public void MergeSortedChunks_ShouldPreserveSortingOrder()
    {
        _comparerMock.Setup(c => c.Compare(It.IsAny<ByteChunkData>(), It.IsAny<ByteChunkData>()))
            .Returns((ByteChunkData x, ByteChunkData y) => x._data.Span.SequenceCompareTo(y._data.Span));

        _merger.MergeSortedChunks(_files);

        using var tempStream = new MemoryStream(_outputStream.ToArray());
        using var reader = new StreamReader(tempStream, Encoding.UTF8);
        string[] resultLines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string[] sortedLines = resultLines.OrderBy(l => l).ToArray();

        Assert.That(resultLines, Is.EqualTo(sortedLines), "Merged file should be sorted");
    }
}

