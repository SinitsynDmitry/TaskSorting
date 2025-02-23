using System.Collections.Immutable;
using System.Text;
using FileCreator.Interfaces;

namespace FileCreator.Parts;

public record GeneratorConfig : IContentStrategyConfig, IGeneratorConfig
{
    /// <summary>
    /// Gets the vocabulary.
    /// </summary>
    public ImmutableArray<string> Vocabulary { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets the partition size.
    /// </summary>
    public int PartitionSize { get; init; } = 1_000_000;

    /// <summary>
    /// Gets the max number part.
    /// </summary>
    public int MaxNumberPart { get; init; } = 5000;

    /// <summary>
    /// Gets the buffer size.
    /// </summary>
    public int BufferSize { get; init; } = 84 * 1024;

    /// <summary>
    /// Gets the encoding.
    /// </summary>
    public Encoding Encoding { get; init; } = Encoding.UTF8;

    /// <summary>
    /// Gets the estimated max row size.
    /// Pre-calculated for performance
    /// </summary>
    public int EstimatedMaxRowSize { get; private init; }

    /// <summary>
    /// Gets the default.
    /// </summary>
    public static GeneratorConfig Default => new();

    /// <summary>
    /// Withs the vocabulary.
    /// </summary>
    /// <param name="vocabulary">The vocabulary.</param>
    /// <returns>A GeneratorConfig.</returns>
    public GeneratorConfig WithVocabulary(string[] vocabulary)
    {
        ArgumentNullException.ThrowIfNull(vocabulary);

        if (vocabulary.Length == 0)
            throw new ArgumentException("Vocabulary array cannot be empty", nameof(vocabulary));
        if (vocabulary.Any(string.IsNullOrEmpty))
            throw new ArgumentException(
                "Vocabulary items cannot be null or empty",
                nameof(vocabulary)
            );

        var immutableVocab = ImmutableArray.Create(vocabulary);
        return this with
        {
            Vocabulary = immutableVocab,
            EstimatedMaxRowSize = CalculateMaxRowSize(immutableVocab, MaxNumberPart),
        };
    }

    /// <summary>
    /// Withs the partition size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>A GeneratorConfig.</returns>
    public GeneratorConfig WithPartitionSize(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        return this with { PartitionSize = size };
    }

    /// <summary>
    /// Withs the buffer size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <returns>A GeneratorConfig.</returns>
    public GeneratorConfig WithBufferSize(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (size < 4096) // Minimum recommended buffer size
            throw new ArgumentException("Buffer size should be at least 4096 bytes", nameof(size));
        return this with { BufferSize = size };
    }

    /// <summary>
    /// Withs the encoding.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <returns>A GeneratorConfig.</returns>
    public GeneratorConfig WithEncoding(Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        return this with { Encoding = encoding };
    }

    /// <summary>
    /// Withs the max number part.
    /// </summary>
    /// <param name="max">The max.</param>
    /// <returns>A GeneratorConfig.</returns>
    public GeneratorConfig WithMaxNumberPart(int max)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max);
        return this with { MaxNumberPart = max };
    }

    /// <summary>
    /// Calculates the max row size.
    /// </summary>
    /// <param name="vocab">The vocab.</param>
    /// <param name="maxNumber">The max number.</param>
    /// <returns>An int.</returns>
    private static int CalculateMaxRowSize(ImmutableArray<string> vocab, int maxNumber) =>
        maxNumber.ToString().Length + (vocab.IsEmpty ? 0 : vocab.Max(s => s.Length));
}
