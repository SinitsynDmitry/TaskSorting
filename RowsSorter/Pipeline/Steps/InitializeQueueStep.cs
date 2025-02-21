using RowsSorter.Interfaces;
using RowsSorter.Pipeline.Contexts;

namespace RowsSorter.Pipeline.Steps;

public class InitializeQueueStep : IProcessingStep<MergingPipelineContext>
{
    /// <summary>
    /// Processes the.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Process(MergingPipelineContext context)
    {
        for (int i = 0; i < context.Readers.Count; i++)
        {
            var line = context.Readers.ReadLine(i, context.Buffer);
            if (line is not null)
            {
                context.SortingQueue.Enqueue(line.Value, i);
            }
        }
    }

    /// <summary>
    /// Processes the async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask ProcessAsync(MergingPipelineContext context)
    {
        for (int i = 0; i < context.Readers.Count; i++)
        {
            var line = await context.Readers.ReadLineAsync(i, context.Buffer);
            if (line is not null)
            {
                context.SortingQueue.Enqueue(line.Value, i);
            }
        }
    }
}
