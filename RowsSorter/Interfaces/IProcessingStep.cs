using RowsSorter.Pipeline;

namespace RowsSorter.Interfaces;

public interface IProcessingStep<T>
{
    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask ProcessAsync(T context);

    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    void Process(T context);
}


