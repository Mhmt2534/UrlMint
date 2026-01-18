
using System.Threading.Channels;
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.BackgroundTasks
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            // Bounded channel: Kuyruk çok dolarsa bekletir, hafızayı şişirmez.
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
