﻿namespace RowsSorter;
internal class ReadOnlyMemoryByteComparer2 : IComparer<ReadOnlyMemory<byte>>
{

    private readonly byte _separator;

    public ReadOnlyMemoryByteComparer2(byte separator = (byte)'.')
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
        var span1 = array1.Span;
        var span2 = array2.Span;

        int fullCompare = span1.SequenceCompareTo(span2);
        if (fullCompare == 0)
        {
            return 0;
        }

        int array1SeparatorIndex = span1.IndexOf(_separator);
        int array2SeparatorIndex = span2.IndexOf(_separator);

        int lengthComparison = (array1.Length - array1SeparatorIndex).CompareTo(array2.Length - array2SeparatorIndex);
        if (lengthComparison != 0)
        {
            return lengthComparison;
        }

        // Compare second segments
        ReadOnlySpan<byte> second1 = array1SeparatorIndex >= 0
            ? span1.Slice(array1SeparatorIndex + 1)
            : ReadOnlySpan<byte>.Empty;
        ReadOnlySpan<byte> second2 = array2SeparatorIndex >= 0
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
