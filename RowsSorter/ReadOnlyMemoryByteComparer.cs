namespace RowsSorter;
internal class ReadOnlyMemoryByteComparer : IComparer<ReadOnlyMemory<byte>>
{

    private readonly byte _separator;

    public ReadOnlyMemoryByteComparer(byte separator = (byte)'.')
    {
        _separator = separator;
    }

    /// <summary>
    /// Compares the.
    /// </summary>
    /// <param name="array1">The array1.</param>
    /// <param name="array2">The array2.</param>
    /// <returns>An int.</returns>
    public int Compare(ReadOnlyMemory<byte> array1, ReadOnlyMemory<byte> array2)
    {
        var (first1Segment, second1Segment) = SplitByteArray(array1);
        var (first2Segment, second2Segment) = SplitByteArray(array2);

        int secondCompare = CompareMemoryBytes(second1Segment, second2Segment);
        if (secondCompare != 0)
        {
            return secondCompare;
        }

        return CompareMemoryBytes(first1Segment, first2Segment);
    }

    /// <summary>
    /// Compares the memory bytes.
    /// </summary>
    /// <param name="array1">The array1.</param>
    /// <param name="array2">The array2.</param>
    /// <returns>An int.</returns>
    private static int CompareMemoryBytes(ReadOnlyMemory<byte> array1, ReadOnlyMemory<byte> array2)
    {
        int lengthComparison = array1.Length.CompareTo(array2.Length);
        if (lengthComparison != 0)
        {
            return lengthComparison;
        }

        ReadOnlySpan<byte> span1 = array1.Span;
        ReadOnlySpan<byte> span2 = array2.Span;

        return span1.SequenceCompareTo(span2);
    }

    /// <summary>
    /// Splits the byte array.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>A (ReadOnlyMemory&lt;byte&gt; firstSegment, ReadOnlyMemory&lt;byte&gt; secondSegment) .</returns>
    private (ReadOnlyMemory<byte> firstSegment, ReadOnlyMemory<byte> secondSegment) SplitByteArray(ReadOnlyMemory<byte> data)
    {
        int separatorIndex = data.Span.IndexOf(_separator);

        return separatorIndex >= 0
            ? (data.Slice(0, separatorIndex), data.Slice(separatorIndex + 1))
            : (data, ReadOnlyMemory<byte>.Empty);
    }
}
