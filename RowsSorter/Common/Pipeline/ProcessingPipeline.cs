namespace RowsSorter.Common.Pipeline;

public class ProcessingPipeline<T> : IPipeline<T>
{
    private readonly List<IProcessingStep<T>> _steps = new();

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>An IPipeline.</returns>
    public IPipeline<T> AddStep(IProcessingStep<T> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ExecuteAsync(T context)
    {
        foreach (var step in _steps)
        {
            await step.ProcessAsync(context);
        }
    }

    /// <summary>
    /// Executes the.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A list of string.</returns>
    public void Execute(T context)
    {
        foreach (var step in _steps)
        {
            step.Process(context);
        }
    }
}


