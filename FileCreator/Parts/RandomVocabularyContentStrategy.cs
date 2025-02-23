using System.Runtime.CompilerServices;
using System.Text;
using FileCreator.Interfaces;

namespace FileCreator.Parts;

public class RandomVocabularyContentStrategy : IContentStrategy
{
    private readonly IContentStrategyConfig _config;
    private readonly IRandomGenerator _random;

    public RandomVocabularyContentStrategy(
        IContentStrategyConfig config,
        IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentOutOfRangeException.ThrowIfEqual(config.Vocabulary.Length, 0);

        ArgumentNullException.ThrowIfNull(random);
        _config = config;
        _random = random;
    }

    /// <summary>
    /// Appends the content.
    /// </summary>
    /// <param name="localStringBuffer">The local string buffer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FillBuffer(StringBuilder localStringBuffer)
    {
        int rowInt = _random.Next(1, _config.MaxNumberPart);
        int vocabularyIndex = _random.Next(0, _config.Vocabulary.Length);
        localStringBuffer.Append(rowInt).Append(_config.Vocabulary[vocabularyIndex]);
    }
}

