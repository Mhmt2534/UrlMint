
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.BackgroundTasks
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueuedHostedService> _logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, IServiceProvider serviceProvider, ILogger<QueuedHostedService> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Uygulama kapanana kadar döngü devam eder
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Take the job from the queue
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    // 2. Create a new scope. 
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // 3. Run the job and sen Scope's Provider to work
                        await workItem(scope.ServiceProvider, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured in the background service.");
                }
            }
        }
    }
}
