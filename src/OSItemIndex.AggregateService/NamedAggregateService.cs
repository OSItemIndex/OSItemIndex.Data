using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OSItemIndex.AggregateService
{
    /// <summary>
    ///
    /// </summary>
    public abstract class NamedAggregateService : IHostedService, IDisposable
    {
        private readonly string _name;
        private readonly TimeSpan? _executeDelay;

        private Task _loopingTask;
        private Task _executingTask;
        private CancellationTokenSource _stoppingCts;

        protected NamedAggregateService(string name, TimeSpan? executeDelay = null)
        {
            _name = name;
            _executeDelay = executeDelay;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Create linked token to allow cancelling executing task from provided token
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            _loopingTask = LoopAsync(_stoppingCts.Token);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            return _loopingTask.IsCompleted ? _loopingTask : Task.CompletedTask; // Otherwise it's running
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancellationToken">Indicates that the loop process should be aborted.</param>
        private async Task<Task> LoopAsync(CancellationToken cancellationToken)
        {
            Log.Information("{@_name} aggregate service is starting", _name);

            cancellationToken.Register(() => Log.Information("{@_name} aggregate service is stopping", _name));

            while (!cancellationToken.IsCancellationRequested)
            {
                _executingTask = ExecuteAsync(_stoppingCts.Token); // Store the task we're executing
                if (_executeDelay != null)
                {
                    await Task.Delay(_executeDelay.Value, cancellationToken);
                }
            }

            Log.Information("{@_name} aggregate service is stopping", _name);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask; // Otherwise it's running
        }

        /// <summary>
        /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_loopingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_loopingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        public virtual void Dispose()
        {
            _stoppingCts?.Cancel();
        }
    }
}
