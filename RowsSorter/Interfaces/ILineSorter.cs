using RowsSorter.Entities;

namespace RowsSorter.Interfaces;

public interface ILineSorter
{
    /// <summary>
    /// Sorts the.
    /// </summary>
    /// <param name="lines">The lines.</param>
    void Sort(List<ByteChunkData> lines);
}
