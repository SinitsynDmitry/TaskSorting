using Moq;
using NUnit.Framework;
using RowsSorter.Entities;
using RowsSorter.MainPipelineSteps.ChunkMerge.Parts;
using System.Collections.Generic;

namespace RowsSorterTests.MainPipelineSteps.ChunkMerge.Parts;

[TestFixture]
public class StreamMergeQueueTests
{
    private StreamMergeQueue _queue;
    private Mock<IComparer<ByteChunkData>> _comparerMock;

    [SetUp]
    public void Setup()
    {
        _comparerMock = new Mock<IComparer<ByteChunkData>>();
        _comparerMock.Setup(c => c.Compare(It.IsAny<ByteChunkData>(), It.IsAny<ByteChunkData>()))
        .Returns((ByteChunkData x, ByteChunkData y) => x._data.Span.SequenceCompareTo(y._data.Span));
        _queue = new StreamMergeQueue(_comparerMock.Object);
    }

    [Test]
    public void Enqueue_ShouldIncreaseCount()
    {
        var data = new TaskItem( new ByteChunkData(new byte[] { 1, 2, 3 }),1);
        _queue.Enqueue(data);
        Assert.That(_queue.Count, Is.EqualTo(1));
    }

    [Test]
    public void Dequeue_ShouldReturnCorrectItem()
    {
        var data1 = new TaskItem(new ByteChunkData(new byte[] { 1, 2, 3 }),0);
        var data2 = new TaskItem(new ByteChunkData(new byte[] { 4, 5, 6 }),1);
        var data3 = new TaskItem(new ByteChunkData(new byte[] { 7, 8, 9 }),2);
        _queue.Enqueue(data2);
        _queue.Enqueue(data3);
        _queue.Enqueue(data1);
        var dequeued = _queue.Dequeue();
        Assert.That(dequeued, Is.EqualTo(data1));
        dequeued = _queue.Dequeue();
        Assert.That(dequeued, Is.EqualTo(data2));
        _queue.Enqueue(data1);
        dequeued = _queue.Dequeue();
        Assert.That(dequeued, Is.EqualTo(data1));
        dequeued = _queue.Dequeue();
        Assert.That(dequeued, Is.EqualTo(data3));
    }

    [Test]
    public void Clear_ShouldResetCount()
    {
        _queue.Enqueue(new TaskItem(new ByteChunkData(new byte[] { 1, 2, 3 }), 0));
        _queue.Clear();
        Assert.That(_queue.Count, Is.EqualTo(0));
    }

    [Test]
    public void Dequeue_FromEmptyQueue_ShouldThrowException()
    {
        Assert.Throws<InvalidOperationException>(() => _queue.Dequeue());
    }

    [Test]
    public void Enqueue_DuplicateValues_ShouldHandleCorrectly()
    {
        var data1 = new TaskItem(new ByteChunkData(new byte[] { 1, 2, 3 }),0);
        var data2 = new TaskItem(new ByteChunkData(new byte[] { 1, 2, 3 }), 0);
        _queue.Enqueue(data1);
        _queue.Enqueue(data2);
        Assert.That(_queue.Count, Is.EqualTo(2));
    }

    [Test]
    public void Enqueue_MultipleItems_ShouldDequeueInCorrectOrder()
    {
        var data1 = new TaskItem(new ByteChunkData(new byte[] { 3, 3, 3 }),2);
        var data2 = new TaskItem(new ByteChunkData(new byte[] { 2, 2, 2 }), 1);
        var data3 = new TaskItem(new ByteChunkData(new byte[] { 1, 1, 1 }), 0);

        _queue.Enqueue(data1);
        _queue.Enqueue(data2);
        _queue.Enqueue(data3);

        Assert.That(_queue.Dequeue(), Is.EqualTo(data3));
        Assert.That(_queue.Dequeue(), Is.EqualTo(data2));
        Assert.That(_queue.Dequeue(), Is.EqualTo(data1));
    }

    [Test]
    public void Clear_ThenEnqueue_ShouldWorkCorrectly()
    {
        _queue.Enqueue(new TaskItem(new ByteChunkData(new byte[] { 1, 2, 3 }), 0));
        _queue.Clear();
        _queue.Enqueue(new TaskItem(new ByteChunkData(new byte[] { 4, 5, 6 }), 1));
        Assert.That(_queue.Count, Is.EqualTo(1));
    }
}
