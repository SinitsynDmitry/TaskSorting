using RowsSorter.Pipeline.Contexts;

namespace RowsSorterTests.Pipeline.Contexts;

[TestFixture]
public class ChunkProcessingContextTests
{
    [Test]
    public void Create_ShouldArgumentNullException_WhenInputFileIsNullOrWhiteSpace()
    {
        // Arrange
        string inputFile = null;
        string outputFile = "output.txt";
        long startPosition = 0;
        int chunkSize = 1024;

        Assert.Throws<ArgumentNullException>(() => ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize));
    }

    [Test]
    public void Create_ShouldArgumentNullException_WhenOutputFileIsNullOrWhiteSpace()
    {
        // Arrange
        string inputFile = "input.txt";
        string outputFile = null;
        long startPosition = 0;
        int chunkSize = 1024;

        Assert.Throws<ArgumentNullException>(() => ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize));
    }

    [Test]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenStartPositionIsNegative()
    {
        // Arrange
        string inputFile = "input.txt";
        string outputFile = "output.txt";
        long startPosition = -1;
        int chunkSize = 1024;

        Assert.Throws<ArgumentOutOfRangeException>(() => ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize));
    }

    [Test]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenChunkSizeIsZeroOrNegative()
    {
        // Arrange
        string inputFile = "input.txt";
        string outputFile = "output.txt";
        long startPosition = 0;
        int chunkSize = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize));
    }

    [Test]
    public void Create_ShouldAllocateBufferCorrectly()
    {
        // Arrange
        string inputFile = "input.txt";
        string outputFile = "output.txt";
        long startPosition = 0;
        int chunkSize = 1024;

        // Act
        var context = ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize);

        // Assert
        Assert.That(context.Buffer, Is.Not.Null);
        Assert.That(context.Buffer.Length, Is.EqualTo(chunkSize));

        // Clean up
        context.ReleaseBuffer();
    }

    [Test]
    public void ReleaseBuffer_ShouldNullifyBufferAfterReturn()
    {
        // Arrange
        string inputFile = "input.txt";
        string outputFile = "output.txt";
        long startPosition = 0;
        int chunkSize = 1024;

        var context = ChunkProcessingContext.Create(inputFile, outputFile, startPosition, chunkSize);

        context.ReleaseBuffer();

        Assert.That(context.Buffer, Is.Null);
    }
}
