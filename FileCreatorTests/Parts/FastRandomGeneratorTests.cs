

using NUnit.Framework;
using FileCreator.Parts;

namespace FileCreatorTests.Parts;

[TestFixture]
public class FastRandomGeneratorTests
{
    private FastRandomGenerator _randomGenerator;

    /// <summary>
    /// Sets up the test environment.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _randomGenerator = new FastRandomGenerator(42); // You can use any seed for consistency
    }

    /// <summary>
    /// Tests if the Next method produces values within the given range.
    /// </summary>
    [Test]
    public void Next_GivenRange_ReturnsValueInRange()
    {
        // Act
        int value = _randomGenerator.Next(1, 10);

        // Assert
        Assert.That(value, Is.GreaterThanOrEqualTo(1)); // Value should be >= 1
        Assert.That(value, Is.LessThan(10)); // Value should be < 10
    }

    /// <summary>
    /// Tests if the Next method consistently generates random numbers given the same seed.
    /// </summary>
    [Test]
    public void Next_SameSeed_ReturnsSameValues()
    {
        var randomGenerator1 = new FastRandomGenerator(42);
        var randomGenerator2 = new FastRandomGenerator(42);


        int value1 = randomGenerator1.Next(1, 10);
        int value2 = randomGenerator2.Next(1, 10);


        Assert.That(value1, Is.EqualTo(value2)); // Same seed should produce same values
    }

    /// <summary>
    /// Tests if the Next method produces different values for different seeds.
    /// </summary>
    [Test]
    public void Next_DifferentSeeds_ReturnsDifferentValues()
    {

        var randomGenerator1 = new FastRandomGenerator(42);
        var randomGenerator2 = new FastRandomGenerator(100);

        int value1 = randomGenerator1.Next(1, 10);
        int value2 = randomGenerator2.Next(1, 10);

        Assert.That(value1, Is.Not.EqualTo(value2)); // Different seeds should produce different values
    }

    /// <summary>
    /// Tests if Next method throws an exception when minValue >= maxValue.
    /// </summary>
    [Test]
    public void Next_InvalidRange_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _randomGenerator.Next(10, 10)); // minValue cannot be equal to or greater than maxValue
    }
}
