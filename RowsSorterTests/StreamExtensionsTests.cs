﻿using NUnit.Framework.Legacy;
using RowsSorter.Extensions;

namespace RowsSorterTests;

[TestFixture]
public class StreamExtensionsTests
{
    private MemoryStream _stream;

    /// <summary>
    /// Setups the.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _stream = new MemoryStream();
    }

    /// <summary>
    /// Tears the down.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _stream.Dispose();
    }

    /// <summary>
    /// Indices the of_ finds byte.
    /// </summary>
    [Test]
    public void IndexOf_FindsByte()
    {
        byte[] data = { 1, 2, 3, 4, 5 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        Span<byte> buffer = new byte[5];
        long position = _stream.IndexOf(0, 3, buffer);
        Assert.That(position, Is.EqualTo(3));
    }

    /// <summary>
    /// Reads the line_ returns line.
    /// </summary>
    [Test]
    public void ReadLine_ReturnsLine()
    {
        byte[] data = { 65, 66, 67, 10, 68, 69 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        Span<byte> buffer = new byte[10];
        var line = _stream.ReadLine(buffer,10);
        CollectionAssert.AreEqual(new byte[] { 65, 66, 67, 10 }, line?.ToArray());
    }

    /// <summary>
    /// Indices the of async_ finds byte.
    /// </summary>
    /// <returns>A Task.</returns>
    [Test]
    public async Task IndexOfAsync_FindsByte()
    {
        byte[] data = { 1, 2, 3, 4, 5 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        Memory<byte> buffer = new byte[2];
        long position = await _stream.IndexOfAsync(0, 3, buffer);
        Assert.That(position, Is.EqualTo(2));
    }

    /// <summary>
    /// Reads the line async_ returns line.
    /// </summary>
    /// <returns>A Task.</returns>
    [Test]
    public async Task ReadLineAsync_ReturnsLine()
    {
        byte[] data = { 65, 66, 67, 10, 68, 69 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        Memory<byte> buffer = new byte[10];
        var line = await _stream.ReadLineAsync(buffer,10);
        CollectionAssert.AreEqual(new byte[] { 65, 66, 67, 10 }, line?.ToArray());
    }

    /// <summary>
    /// Reads the chunk_ returns expected bytes.
    /// </summary>
    [Test]
    public void ReadChunk_ReturnsExpectedBytes()
    {
        byte[] data = { 1, 2, 3, 4, 5 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        var chunk = _stream.ReadChunk(1, 3);
        CollectionAssert.AreEqual(new byte[] { 2, 3, 4 }, chunk.ToArray());
    }

    /// <summary>
    /// Reads the chunk async_ returns expected bytes.
    /// </summary>
    /// <returns>A Task.</returns>
    [Test]
    public async Task ReadChunkAsync_ReturnsExpectedBytes()
    {
        byte[] data = { 1, 2, 3, 4, 5 };
        _stream.Write(data, 0, data.Length);
        _stream.Position = 0;

        var chunk = await _stream.ReadChunkAsync(1, 3);
        CollectionAssert.AreEqual(new byte[] { 2, 3, 4 }, chunk.ToArray());
    }
}
