
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.BackgroundTasks
{
    public class ExpiredUrlCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ILogger<ExpiredUrlCleanupService> _logger;
        public ExpiredUrlCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredUrlCleanupService> logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider
                        .GetRequiredService<IShortUrlRepository>();
                    await repository.DeleteOldExpiredUrlsAsync();
                    await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ExpiredUrlCleanupService hata aldı");
                }


            }
        }

    }
}
