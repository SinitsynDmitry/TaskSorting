using System.Buffers;
using System.Collections.Immutable;
using RowsSorter.Common.Interfaces;
using RowsSorter.MainPipelineSteps.ChunkProcessing.Context;

namespace RowsSorter.MainPipelineSteps.ChunkProcessing.Steps;

public class LineSorter : ILineSorter
{
    private readonly IComparer<ReadOnlySpan<byte>> _comparer;
    private readonly ThreadLocal<List<(int start, int length)>> _reusableLines = new(
        () => new List<(int start, int length)>()
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="LineSorter"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public LineSorter(IComparer<ReadOnlySpan<byte>> comparer)
    {
        _comparer = comparer;
    }

    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="buffer">The _buffer.</param>
    /// <param name="totalSize">The total size.</param>
    public void Sort(Memory<byte> buffer, int totalSize)
    {
        var threadLocalLines = _reusableLines.Value;
        threadLocalLines!.Clear();

        GetLinePositions(buffer, totalSize, threadLocalLines, (byte)'\n');

        threadLocalLines.Sort(
            (a, b) =>
                _comparer.Compare(
                    buffer.Span.Slice(a.start, a.length),
                    buffer.Span.Slice(b.start, b.length)
                )
        );

        SortIntoBuffer(buffer, totalSize, threadLocalLines);
    }

    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Sort(ChunkProcessingContext context)
    {
        Sort(context.Buffer, context.BytesRead);
    }

    /// <summary>
    /// Gets the line positions.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <param name="length">The length.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="separator">The separator.</param>
    private void GetLinePositions(
        Memory<byte> chunk,
        int length,
        List<(int start, int length)> lines,
        byte separator
    )
    {
        int position = 0;

        while (position < length)
        {
            int separatorIndex = chunk[position..].Span.IndexOf(separator);
            if (separatorIndex == -1)
            {
                lines.Add((position, chunk.Length - position));
                break;
            }

            lines.Add((position, separatorIndex + 1));
            position += separatorIndex + 1;
        }
    }

    /// <summary>
    /// Sorts the into _buffer.
    /// </summary>
    /// <param name="buffer">The _buffer.</param>
    /// <param name="totalSize">The total size.</param>
    /// <param name="lines">The lines.</param>
    private void SortIntoBuffer(
        Memory<byte> buffer,
        int totalSize,
        IReadOnlyList<(int start, int length)> lines
    )
    {
        var tempBuffer = ArrayPool<byte>.Shared.Rent(totalSize);
        try
        {
            int position = 0;
            foreach (var (start, length) in lines)
            {
                buffer.Span.Slice(start, length).CopyTo(tempBuffer.AsSpan(position));
                position += length;
            }
            tempBuffer.AsSpan(0, position).CopyTo(buffer.Span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer, clearArray: true);
        }
    }
}
