﻿using RowsSorter.Common.Pipeline;
using RowsSorter.MainPipelineSteps.ChunkMerge.Context;

namespace RowsSorter.MainPipelineSteps.ChunkMerge.Mergers.Steps
{
    public class ProcessQueueStep : IProcessingStep<MergingPipelineContext>
    {
        /// <summary>
        /// Processes the.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Process(MergingPipelineContext context)
        {
            while (context.SortingQueue.Count > 0)
            {
                var taskItem = context.SortingQueue.Dequeue();
                taskItem.Value.WriteTo(context.Writer);
                var index = taskItem.Index;

                var line = context.Readers.ReadLine(index, context.Buffer);
                if (line is not null)
                {
                    taskItem.Value = line.Value;
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
            while (context.SortingQueue.Count > 0)
            {
                var taskItem = context.SortingQueue.Dequeue();
                taskItem.Value.WriteTo(context.Writer);
                var index = taskItem.Index;

                var line = await context.Readers.ReadLineAsync(index, context.Buffer);
                if (line is not null)
                {
                    taskItem.Value = line.Value;
                    context.SortingQueue.Enqueue(taskItem);
                }
            }
        }
    }
}
