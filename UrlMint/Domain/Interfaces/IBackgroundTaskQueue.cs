namespace UrlMint.Domain.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        // method that adds work to the queue
        ValueTask QueueBackgroundWorkItemAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem);

        // Queue-based processing method
        ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
    }
}
