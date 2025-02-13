using RowsSorter.Entities;

namespace RowsSorter.Extensions;
public static class ReadOnlyMemoryByteExtensions
{

    /// <summary>
    /// Splits the into lines.
    /// </summary>
    /// <param name="chunkBytes">The chunk bytes.</param>
    /// <param name="lines">The lines.</param>
    /// <param name="separator">The separator.</param>
    public static void SplitIntoLines(this ReadOnlyMemory<byte> chunkBytes, List<ByteChunkData> lines, byte separator = (byte)'\n')
    {
        int position = 0;
        ReadOnlySpan<byte> span = chunkBytes.Span;

        while (position < span.Length)
        {
            int separatorIndex = span.Slice(position).IndexOf(separator);

            if (separatorIndex == -1)
            {
                lines.Add(new ByteChunkData(chunkBytes[position..]));
                break;
            }

            lines.Add(new ByteChunkData(chunkBytes.Slice(position, separatorIndex + 1)));
            position += separatorIndex + 1;
        }
    }
}
