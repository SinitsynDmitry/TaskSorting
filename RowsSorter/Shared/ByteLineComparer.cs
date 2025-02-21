namespace RowsSorter.Shared;

public class ByteLineComparer : IComparer<ReadOnlySpan<byte>>, IComparer<ReadOnlyMemory<byte>>
{
    private readonly byte _separator;


    public ByteLineComparer(byte separator = (byte)'.')
    {
        _separator = separator;
    }

    /// <summary>
    /// Compares the.
    /// </summary>
    /// <param name="memory1">The memory1.</param>
    /// <param name="memory2">The memory2.</param>
    /// <returns>An int.</returns>
    public int Compare(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
    {
        return Compare(memory1.Span, memory2.Span);
    }

    /// <summary>
    /// Compares the.
    /// </summary>
    /// <param name="span1">The span1.</param>
    /// <param name="span2">The span2.</param>
    /// <returns>An int.</returns>
    public int Compare(ReadOnlySpan<byte> span1, ReadOnlySpan<byte> span2)
    {
        int fullCompare = span1.SequenceCompareTo(span2);
        if (fullCompare == 0)
        {
            return 0;
        }

        int array1SeparatorIndex = span1.IndexOf(_separator);
        int array2SeparatorIndex = span2.IndexOf(_separator);

        int lengthComparison = (span1.Length - array1SeparatorIndex).CompareTo(
            span2.Length - array2SeparatorIndex
        );
        if (lengthComparison != 0)
        {
            return lengthComparison;
        }

        // Compare second segments
        ReadOnlySpan<byte> second1 =
            array1SeparatorIndex >= 0
                ? span1.Slice(array1SeparatorIndex + 1)
                : ReadOnlySpan<byte>.Empty;
        ReadOnlySpan<byte> second2 =
            array2SeparatorIndex >= 0
                ? span2.Slice(array2SeparatorIndex + 1)
                : ReadOnlySpan<byte>.Empty;

        int secondCompare = second1.SequenceCompareTo(second2);
        if (secondCompare != 0)
        {
            return secondCompare;
        }

        lengthComparison = array1SeparatorIndex.CompareTo(array2SeparatorIndex);
        if (lengthComparison != 0)
        {
            return lengthComparison;
        }

        return fullCompare;
    }
}
