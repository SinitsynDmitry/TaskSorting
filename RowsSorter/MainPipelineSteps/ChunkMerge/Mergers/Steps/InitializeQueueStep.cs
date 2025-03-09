using RowsSorter.Entities;
using System;
using RowsSorter.Common.Pipeline;
using RowsSorter.MainPipelineSteps.ChunkMerge.Context;

namespace RowsSorter.MainPipelineSteps.ChunkMerge.Mergers.Steps;

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
                var taskItem = new TaskItem(line.Value, i);
                context.SortingQueue.Enqueue(taskItem);
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
                var taskItem = new TaskItem(line.Value, i);
                context.SortingQueue.Enqueue(taskItem);
            }
        }
    }
}
