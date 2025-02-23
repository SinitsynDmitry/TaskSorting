using System.Text;
using FileCreator.Parts;

namespace FileCreator.Interfaces;

public interface IGeneratorConfig
{
    /// <summary>
    /// Gets the buffer size.
    /// </summary>
    int BufferSize { get; init; }
    /// <summary>
    /// Gets the encoding.
    /// </summary>
    Encoding Encoding { get; init; }
    /// <summary>
    /// Gets the estimated max row size.
    /// </summary>
    int EstimatedMaxRowSize { get; }
    /// <summary>
    /// Gets the partition size.
    /// </summary>
    int PartitionSize { get; init; }

    /// <summary>
    /// Withs the vocabulary.
    /// </summary>
    /// <param name="vocabulary">The vocabulary.</param>
    /// <returns>A GeneratorConfig.</returns>
    GeneratorConfig WithVocabulary(string[] vocabulary);
}

