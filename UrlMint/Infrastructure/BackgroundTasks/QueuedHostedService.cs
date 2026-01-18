
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
                    // 1. Kuyruktan işi al
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    // 2. Yeni bir Scope (alan) oluştur. 
                    // Önceki cevabımdaki ScopeFactory mantığı burada çalışacak.
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // 3. İşi çalıştır ve Scope'un Provider'ını işe gönder
                        await workItem(scope.ServiceProvider, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Arka plan servisinde hata oluştu.");
                }
            }
        }
    }
}
