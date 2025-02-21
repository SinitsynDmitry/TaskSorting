using RowsSorter.Shared;

namespace RowsSorterTests;

[TestFixture]
public class TemporaryFileManagerTests
{
    private TemporaryFileManager _manager;
    private TempFileCollection _files;

    [SetUp]
    public void SetUp()
    {
        _files = new TempFileCollection
        {
            Chunks = new List<string> { "chunk1.tmp", "chunk2.tmp", "chunk3.tmp" },
            OutputFile = "output.tmp"
        };
        _manager = new TemporaryFileManager(_files);
    }

    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        Assert.That(_manager.OutputFile, Is.EqualTo("output.tmp"));
        Assert.That(_manager.Count, Is.EqualTo(3));
    }

    [Test]
    public void Clear_ShouldRemoveAllChunks()
    {
        _manager.Clear();
        Assert.That(_manager.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetTempFileName_ShouldReturnValidPath()
    {
        string fileName = _manager.GetTempFileName(1, 0, 10);
        Assert.That(fileName, Does.Contain("chunk_1_0_10.tmp"));
    }

    [Test]
    public void GetNextMergeBatch_ShouldMergeChunksCorrectly()
    {
        var batches = _manager.GetNextMergeBatch();
        Assert.That(batches.Count, Is.GreaterThan(0));

        foreach (var batch in batches)
        {
            Assert.That(batch.Chunks, Is.Not.Empty);
            Assert.That(batch.OutputFile, Is.Not.Null);
        }
    }
}
