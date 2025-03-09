using Moq;
using RowsSorter.Common.Interfaces;
using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;

namespace RowsSorterTests.MainPipelineSteps.ChunkMerge.Parts;

[TestFixture]
public class StreamCollectionTests
{
    private Mock<IStreamProvider> _streamProviderMock;
    private StreamCollection _streamCollection;
    private string[] _testFiles;

    [SetUp]
    public void SetUp()
    {
        _streamProviderMock = new Mock<IStreamProvider>();
        _testFiles = new[] { "file1.txt", "file2.txt" };

       
        for (int i = 0; i < _testFiles.Length; i++)
        {
            var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"line{i}1\nline{i}2\n"));
            _streamProviderMock.Setup(p => p.GetReadStream(_testFiles[i], It.IsAny<int>(), It.IsAny<bool>())).Returns(memoryStream);
        }

        _streamCollection = new StreamCollection(_testFiles, _streamProviderMock.Object);
    }

    /// <summary>
    /// Tears the down.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _streamCollection?.Dispose();
    }

    [Test]
    public void Count_ShouldReturnCorrectNumberOfStreams()
    {
        Assert.That(_streamCollection.Count, Is.EqualTo(_testFiles.Length));
    }

    [Test]
    public void ReadLine_ShouldReturnFirstLinesFromStreams()
    {
        Span<byte> buffer = stackalloc byte[256];
        for (int i = 0; i < _streamCollection.Count; i++)
        {
            var result = _streamCollection.ReadLine(i, buffer);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value._data.ToArray(), Is.EqualTo(System.Text.Encoding.UTF8.GetBytes($"line{i}1\n")));
        }
    }

    [Test]
    public async Task ReadLineAsync_ShouldReturnFirstLinesFromStreams()
    {
        Memory<byte> buffer = new byte[256];
        for (int i = 0; i < _streamCollection.Count; i++)
        {
            var result = await _streamCollection.ReadLineAsync(i, buffer);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value._data.ToArray(), Is.EqualTo(System.Text.Encoding.UTF8.GetBytes($"line{i}1\n")));
        }
    }



    [Test]
    public void Dispose_ShouldCloseAllStreams()
    {
        _streamCollection.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _streamCollection.ReadLine(0, stackalloc byte[256]));
    }
}
