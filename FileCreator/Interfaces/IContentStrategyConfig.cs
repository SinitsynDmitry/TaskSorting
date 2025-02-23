using System.Collections.Immutable;
using FileCreator.Parts;

namespace FileCreator.Interfaces;

public interface IContentStrategyConfig
{
    /// <summary>
    /// Gets the max number part.
    /// </summary>
    int MaxNumberPart { get; init; }

    /// <summary>
    /// Gets the vocabulary.
    /// </summary>
    ImmutableArray<string> Vocabulary { get; }

    /// <summary>
    /// Withs the vocabulary.
    /// </summary>
    /// <param name="vocabulary">The vocabulary.</param>
    /// <returns>A GeneratorConfig.</returns>
    GeneratorConfig WithVocabulary(string[] vocabulary);
}

