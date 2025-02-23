using FileCreator.Interfaces;

namespace FileCreator.Parts;

public class RandomGenerator : IRandomGenerator
{
    private static readonly ThreadLocal<Random> _threadLocalRandom =
        new(() => new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId));

    /// <summary>Returns a random integer that is within a specified range.</summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>An int.</returns>
    public int Next(int minValue, int maxValue)
    {
        var random = _threadLocalRandom.Value!;
        return random.Next(minValue, maxValue);
    }
}

