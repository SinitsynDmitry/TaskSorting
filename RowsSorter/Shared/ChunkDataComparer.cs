using RowsSorter.Entities;

namespace RowsSorter.Shared;

public class ChunkDataComparer : IComparer<ByteChunkData>
{
    private readonly IComparer<ReadOnlyMemory<byte>> _dataComparer;

    public ChunkDataComparer(IComparer<ReadOnlyMemory<byte>> dataComparer)
    {
        ArgumentNullException.ThrowIfNull(dataComparer);
        _dataComparer = dataComparer;
    }

    /// <summary>
    /// Compares the.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>An int.</returns>
    public int Compare(ByteChunkData x, ByteChunkData y)
    {
        return _dataComparer.Compare(x._data, y._data);
    }
}
