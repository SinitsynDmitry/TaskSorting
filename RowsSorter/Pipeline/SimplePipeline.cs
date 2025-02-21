using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Pipeline;

public class SimplePipeline : IPipeline<FileProcessingContext, List<string>>
{
    private readonly List<IProcessingStep<FileProcessingContext>> _steps = new();

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>An IPipeline.</returns>
    public IPipeline<FileProcessingContext, List<string>> AddStep(IProcessingStep<FileProcessingContext> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask<List<string>> ExecuteAsync(FileProcessingContext context)
    {
        foreach (var step in _steps)
        {
            await step.ProcessAsync(context);
        }

        return context.TempChunks.Select(f => f.outputFile).ToList();
    }

    /// <summary>
    /// Executes the.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A list of string.</returns>
    public List<string> Execute(FileProcessingContext context)
    {
        foreach (var step in _steps)
        {
            step.Process(context);
        }

        return context.TempChunks.Select(f => f.outputFile).ToList();
    }
}

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


