namespace RowsSorter.Common.Pipeline;

public interface IPipeline<T, U>
{
    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>An IPipeline.</returns>
    IPipeline<T, U> AddStep(IProcessingStep<T> step);

    /// <summary>
    /// Executes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask<U> ExecuteAsync(T context);

    /// <summary>
    /// Executes the.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>An U.</returns>
    U Execute(T context);
}

public interface IPipeline<T>
{
    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>An IPipeline.</returns>
    IPipeline<T> AddStep(IProcessingStep<T> step);
    /// <summary>
    /// Executes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    ValueTask ExecuteAsync(T context);
    /// <summary>
    /// Executes the.
    /// </summary>
    /// <param name="context">The context.</param>
    void Execute(T context);
}