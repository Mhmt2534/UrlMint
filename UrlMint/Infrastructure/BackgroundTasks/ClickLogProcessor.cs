using StackExchange.Redis;
using System.Text.Json;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Infrastructure.Persistence;

namespace UrlMint.Infrastructure.BackgroundTasks
{
    public class ClickLogProcessor : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClickLogProcessor> _logger;

        public ClickLogProcessor(IConnectionMultiplexer redis, IServiceProvider serviceProvider, ILogger<ClickLogProcessor> logger)
        {
            _redis = redis;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                // Kuyruktan veriyi çek (Bloklayıcı okuma yapabilirsin veya polling)
                // LeftPopAsync kuyruğun başından veriyi alır ve siler.
                var rawLog = await db.ListLeftPopAsync("click_queue");

                if (rawLog.HasValue)
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<ClickEventDto>(rawLog);

                        // Scope oluşturup DB'ye yaz (Scoped Service olduğu için)
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<UrlMintDbContext>();

                            // Burada IP Location servisi de çağırabilirsin (GeoIP)

                            var logEntity = new ClickLog
                            {
                                ShortCode = data.ShortCode,
                                IpAddress = data.IpAddress,
                                UserAgent = data.UserAgent,
                                Referer = data.Referer,
                                ClickedAt = data.Timestamp
                                // Parser kullanarak UserAgent'tan OS/Browser ayıklayabilirsin
                            };

                            dbContext.ClickLogs.Add(logEntity);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Log işlenirken hata oluştu.");
                        // Hata olursa veriyi kaybetmemek için kuyruğa geri atma mantığı eklenebilir.
                    }
                }
                else
                {
                    // Kuyruk boşsa işlemciyi biraz dinlendir
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
