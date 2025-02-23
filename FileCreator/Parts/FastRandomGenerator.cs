using System.Runtime.CompilerServices;
using FileCreator.Interfaces;

namespace FileCreator.Parts;

public sealed class FastRandomGenerator : IRandomGenerator
{
    private uint _seed;

    public FastRandomGenerator(int seed = 0)
    {
        _seed = (uint)seed;
    }

    /// <summary>Returns a random integer that is within a specified range.</summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>An int.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minValue, maxValue);
        // XorShift algorithm - faster than System.Random
        _seed ^= _seed << 13;
        _seed ^= _seed >> 17;
        _seed ^= _seed << 5;

        // Scale to range
        uint range = (uint)(maxValue - minValue);
        return minValue + (int)(_seed % range);
    }
}

