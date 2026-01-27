
using StackExchange.Redis;
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.BackgroundTasks
{
    public class UrlStatsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnectionMultiplexer _redisMultiplexer;
        private readonly ILogger<UrlStatsBackgroundService> _logger;

        public UrlStatsBackgroundService(IServiceProvider serviceProvider, IConnectionMultiplexer redisMultiplexer, ILogger<UrlStatsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _redisMultiplexer = redisMultiplexer;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Test için 5 saniye bekle (Sonra bunu 1 dakika yaparsın)
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                    await SyncClickCountsToDatabaseAsync();
                }
                catch (OperationCanceledException)
                {
                    // Uygulama kapanırken buraya düşmesi normaldir via stoppingToken
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background worker hata aldı.");
                }
            }
        }


        private async Task SyncClickCountsToDatabaseAsync()
        {
            var server = _redisMultiplexer.GetServer(_redisMultiplexer.GetEndPoints().First());
            var db = _redisMultiplexer.GetDatabase();

            // "stats:click:*" ile başlayan anahtarları bul
            var keys = server.Keys(pattern: "stats:click:*");

            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IShortUrlRepository>();

                foreach (var key in keys)
                {
                    // Değeri al ve Redis'ten sil (Atomic)
                    var countValue = await db.StringGetDeleteAsync(key);

                    if (countValue.HasValue && long.TryParse(countValue, out long countToAdd))
                    {
                        var code = key.ToString().Split(':').Last(); // "stats:click:4qq" -> "4qq"
                        await repository.BatchUpdateClickCountAsync(code, countToAdd);
                        _logger.LogInformation($"Synced {countToAdd} clicks for {code}");
                    }
                }
            }
        }

    }
}
