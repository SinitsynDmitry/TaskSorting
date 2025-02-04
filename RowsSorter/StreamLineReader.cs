using RowsSorter.Interfaces;

namespace RowsSorter;

internal class StreamLineReader: IStreamLineReader
{
    /// <summary>
    /// Reads the line.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="lineBuffer">The line buffer.</param>
    /// <returns>A bool.</returns>
    public bool ReadLine(Stream reader, Stream lineBuffer)
    {
        lineBuffer.SetLength(0);
        lineBuffer.Position = 0;

        int currentByte;

        while ((currentByte = reader.ReadByte()) != -1)
        {
            if (currentByte == '\n')
            {
                lineBuffer.WriteByte((byte)currentByte);
                break;
            }

            if (currentByte != '\r')
            {
                lineBuffer.WriteByte((byte)currentByte);
            }
        }

        var endOfFile = lineBuffer.Length == 0 && currentByte == -1;
        return endOfFile;
    }

    /// <summary>
    /// Reads the line async.
    /// </summary>
    /// <param name="readerStream">The reader stream.</param>
    /// <param name="lineBuffer">The line buffer.</param>
    /// <returns>A Task.</returns>
    public async Task<bool> ReadLineAsync(Stream readerStream, MemoryStream lineBuffer)
    {
        lineBuffer.SetLength(0);
        lineBuffer.Position = 0;

        int currentByte;
        var singleByte = new byte[1];

        while (await readerStream.ReadAsync(singleByte, 0, 1) > 0)
        {
            currentByte = singleByte[0];

            if (currentByte == '\n')
            {
                lineBuffer.WriteByte((byte)currentByte);
                break;
            }

            if (currentByte != '\r')
            {
                lineBuffer.WriteByte((byte)currentByte);
            }
        }

        // Check for end of file
        bool endOfFile = lineBuffer.Length == 0 && singleByte[0] == 0;

        return endOfFile;
    }
}
