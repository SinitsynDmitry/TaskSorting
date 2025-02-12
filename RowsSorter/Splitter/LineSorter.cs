using RowsSorter.Entities;
using RowsSorter.Interfaces;

namespace RowsSorter.Splitter;

public class LineSorter : ILineSorter
{
    private readonly IComparer<ByteChunkData> _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineSorter"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    public LineSorter(IComparer<ByteChunkData> comparer)
    {
        _comparer = comparer;
    }

    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="lines">The lines.</param>
    public void Sort(List<ByteChunkData> lines)
    {
        lines.Sort(_comparer);
    }
}
