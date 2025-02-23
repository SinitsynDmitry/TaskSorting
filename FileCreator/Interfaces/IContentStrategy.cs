using System.Text;

namespace FileCreator.Interfaces;

public interface IContentStrategy
{
    /// <summary>
    /// Appends the content.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    void FillBuffer(StringBuilder buffer);
}

