using NUnit.Framework;
using Moq;
using System;
using System.IO;
using System.Buffers;
using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorterTests.Pipeline.Contexts;

[TestFixture]
public class MergingPipelineContextTests
{
    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenReadersIsNull()
    {
        // Arrange
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MergingPipelineContext(null, sortingQueue.Object, writer)
        );
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenSortingQueueIsNull()
    {
        // Arrange
        var readers = new Mock<IStreamCollection>();
        var writer = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new MergingPipelineContext(readers.Object, null, writer)
        );
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenWriterIsNull()
    {
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();

        Assert.Throws<ArgumentNullException>(() =>
            new MergingPipelineContext(readers.Object, sortingQueue.Object, null)
        );
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenBufferSizeIsLessZero()
    {
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MergingPipelineContext(readers.Object, sortingQueue.Object, writer,-1)
        );
    }

    [Test]
    public void Constructor_ShouldAllocateBuffer()
    {
        // Arrange
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        // Act
        var context = new MergingPipelineContext(readers.Object, sortingQueue.Object, writer, 256);

        // Assert
        Assert.That(context.Buffer, Is.Not.Null);
        Assert.That(context.Buffer.Length, Is.EqualTo(256));

        // Clean up
        context.Dispose();
    }

    [Test]
    public void Constructor_ShouldDefaultBufferSizeWhenNotProvided()
    {
        // Arrange
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        // Act
        var context = new MergingPipelineContext(readers.Object, sortingQueue.Object, writer);

        // Assert
        Assert.That(context.Buffer, Is.Not.Null);
        Assert.That(context.Buffer.Length, Is.EqualTo(128));  // Default buffer size

        // Clean up
        context.Dispose();
    }

    [Test]
    public void Dispose_ShouldReleaseBufferAndDisposeResources()
    {
        // Arrange
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        var context = new MergingPipelineContext(readers.Object, sortingQueue.Object, writer, 256);

        // Act
        context.Dispose();

        // Assert
        Assert.That(context.Buffer, Is.Null);
        readers.Verify(r => r.Dispose(), Times.Once);
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var x = writer.Length;
        } );
        sortingQueue.Verify(s => s.Clear(), Times.Once);
    }

    [Test]
    public void Dispose_ShouldNotThrowWhenCalledMultipleTimes()
    {
        // Arrange
        var readers = new Mock<IStreamCollection>();
        var sortingQueue = new Mock<ISortingQueue>();
        var writer = new MemoryStream();

        var context = new MergingPipelineContext(readers.Object, sortingQueue.Object, writer, 256);

        // Act & Assert (Dispose multiple times)
        Assert.DoesNotThrow(() =>
        {
            context.Dispose();
            context.Dispose();  // Call dispose again
        });
    }
}
