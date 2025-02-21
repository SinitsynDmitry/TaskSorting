using RowsSorter.Interfaces;

namespace RowsSorter.Pipeline.Steps
{
    public class ProcessingStep<T> : IProcessingStep<T>
    {
        private readonly Action<T> _syncProcess;
        private readonly Func<T, ValueTask> _asyncProcess;

        /// <summary>
        /// Froms the sync only.
        /// </summary>
        /// <param name="syncProcess">The sync process.</param>
        /// <returns>A ProcessingStep.</returns>
        public static ProcessingStep<T> FromSyncOnly<T>(Action<T> syncProcess)
        {
            return new ProcessingStep<T>(syncProcess);
        }

        public ProcessingStep(Action<T> syncProcess, Func<T, ValueTask> asyncProcess)
        {
            _syncProcess = syncProcess ?? throw new ArgumentNullException(nameof(syncProcess));
            _asyncProcess = asyncProcess ?? throw new ArgumentNullException(nameof(asyncProcess));
        }

        public ProcessingStep(Action<T> syncProcess)
        {
            _syncProcess = syncProcess ?? throw new ArgumentNullException(nameof(syncProcess));
            _asyncProcess = ctx =>
            {
                syncProcess(ctx);

                return ValueTask.CompletedTask;
            };
        }

        /// <summary>
        /// Processes the.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Process(T context)
        {
            _syncProcess(context);
        }

        /// <summary>
        /// Processes the async.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A ValueTask.</returns>
        public ValueTask ProcessAsync(T context)
        {
            return _asyncProcess(context);
        }
    }
}
