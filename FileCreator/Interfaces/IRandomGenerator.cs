using System.Runtime.CompilerServices;

namespace FileCreator.Interfaces;

public interface IRandomGenerator
{
    /// <summary>Returns a random integer that is within a specified range.</summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>An int.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int Next(int minValue, int maxValue);
}

