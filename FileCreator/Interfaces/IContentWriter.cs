using System.Text;

namespace FileCreator.Interfaces;
public interface IContentWriter : IDisposable
{
    /// <summary>
    /// Writes the buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    void WriteBuffer(StringBuilder buffer);

    /// <summary>
    /// Writes the buffer async.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask WriteBufferAsync(StringBuilder buffer);
}