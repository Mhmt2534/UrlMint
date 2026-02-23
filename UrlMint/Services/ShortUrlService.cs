using StackExchange.Redis; // Bu namespace gerekli
using Microsoft.Extensions.Caching.Distributed;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;
using UrlMint.Services.Interfaces;
using UrlMint.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UrlMint.Services.Common.Validation;
using UrlMint.Services.Common.Exceptions;
using System.Text.Json;

namespace UrlMint.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _repository;
        private readonly IUrlEncoder _encoder;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redisDb; //Just redis
        private readonly IUrlSafetyService _safetyService;
        private readonly ILogger<ShortUrlService> _logger;

        public ShortUrlService(IShortUrlRepository repository, IUrlEncoder encoder
            , IDistributedCache cache, IConnectionMultiplexer redisMultiplexer
            , IUrlSafetyService safetyService, ILogger<ShortUrlService> logger)
        {
            _repository = repository;
            _encoder = encoder;
            _cache = cache;
            _redisDb = redisMultiplexer.GetDatabase();
            _safetyService = safetyService;
            _logger = logger;
        }

        public async Task<IEnumerable<ShortUrlResponseDto>> GetAllAsync()
        {
            var urls = await _repository.GetAllAsync();

            return urls.Select(ToDto);
        }

        public async Task<ShortUrlResponseDto> GetByIdAsync(string code)
        {
            var id = _encoder.Decode(code);
            var entity = await _repository.GetByIdAsync(id);


            return ToDto(entity);
        }

        #region redirect
        public async Task<string> RedirectToLongUrl(string code, bool isPrefetch, ClickEventDto clickData)
        {

            string cacheKey = $"url:{code}";

            var cachedUrl = await _cache.GetStringAsync(cacheKey);


            if (string.IsNullOrEmpty(cachedUrl))
            {
                var response = await _repository.GetByShortCodeAsync(code);

                if (response == null ||
                    (response.ExpiresAt.HasValue && response.ExpiresAt.Value <= DateTime.UtcNow))
                        return null;


                var ttl = response.ExpiresAt.HasValue
                ? response.ExpiresAt.Value - DateTime.UtcNow
                : TimeSpan.FromHours(24);

                if (ttl <= TimeSpan.Zero) return null;


                cachedUrl = response.LongUrl;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                await _cache.SetStringAsync(cacheKey, cachedUrl, options);
            }


            if (!isPrefetch)
            {
                await _redisDb.StringIncrementAsync($"stats:click:{code}");
                
                var jsonLog = JsonSerializer.Serialize(clickData);

                await _redisDb.ListRightPushAsync("click_queue", jsonLog);

            }
            else
            {
                Console.WriteLine("Preload detected, counter not incremented");
            }

            return cachedUrl;

        }
        #endregion


        public async Task<ShortUrlResponseDto> GetByLongUrlAsync(ShortUrlRequestDto requestDto)
        {
            var entity = await _repository.GetByLongUrlAsync(requestDto.LongUrl);

            return ToDtoWithEncodedCode(entity);
        }


        public async Task<ShortUrlResponseDto> UrlShortener(ShortUrlRequestDto requestDto)
        {
            if (!await _safetyService.IsUrlSafeAsync(requestDto.LongUrl))
            {
                throw new UnsafeUrlException("This URL is not secure (Malware/Phishing detected)");
            }

            var strategy = _repository.GetExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _repository.BeginTransaction();

                var result = await CreateInternalAsync(requestDto); //Create short urls

                await tx.CommitAsync();
                return result;
            });
        }


        public async Task<ShortUrlResponseDto> GetByShortCodeAsync(string code)
        {
            var entity = await _repository.GetByShortCodeAsync(code);
            return ToDto(entity);

        }

        public async Task SeedDataAsync(List<string> longUrls)
        {
            var strategy = _repository.GetExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _repository.BeginTransaction();

                try
                {
                    var entities = longUrls.Select(url => new ShortUrl
                    {
                        LongUrl = url,
                        CreatedAt = DateTime.UtcNow,
                        ClickCount = 0,
                        ShortCode = Guid.NewGuid().ToString().Substring(0, 8)
                    }).ToList();

                    await _repository.CreatBatchAsync(entities);

                    foreach (var entity in entities)
                    {
                        entity.ShortCode = _encoder.Encode(entity.Id);
                    }


                    await _repository.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }


            });
        }


        //Private Helpers
        // Entity with short code
        private ShortUrlResponseDto ToDto(ShortUrl entity)
        {
            if (entity == null) return null;

            return new ShortUrlResponseDto
            {
                ShortCode = entity.ShortCode,
                LongUrl = entity.LongUrl,
                CreatedAt = entity.CreatedAt,
                ExpiresAt = entity.ExpiresAt,
                ClickCount = entity.ClickCount
            };
        }

        // Entity without short code (Id -> encode)
        private ShortUrlResponseDto ToDtoWithEncodedCode(ShortUrl entity)
        {
            if (entity == null) return null;

            return new ShortUrlResponseDto
            {
                ShortCode = _encoder.Encode(entity.Id),
                LongUrl = entity.LongUrl,
                CreatedAt = entity.CreatedAt,
                ExpiresAt = entity.ExpiresAt,
                ClickCount = entity.ClickCount
            };
        }


        private async Task<ShortUrlResponseDto> CreateInternalAsync(ShortUrlRequestDto requestDto)
        {
            string? shortcode = null;

            if (!string.IsNullOrWhiteSpace(requestDto.CustomAlias))
            {
                var alias = requestDto.CustomAlias.ToLower();

                AliasValidator.Validate(alias);

                var exist = await _repository.ExistsAsync(alias);
                if (exist)
                    throw new ConflictException("Alias already in use");


                shortcode = alias;

            }


            var shortUrl = new ShortUrl
            {
                LongUrl = requestDto.LongUrl,
                CreatedAt = DateTime.UtcNow,
                ShortCode = shortcode,
                ClickCount = 0
            };

            try
            {
                var created = await _repository.CreateAsync(shortUrl);
                await _repository.SaveChangesAsync();

                if (shortUrl.ShortCode is null)
                {
                    created.ShortCode = _encoder.Encode(created.Id);
                    await _repository.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueViolation(ex))
            {
                throw new ConflictException("Alias already in use");
            }


            _logger.LogInformation("Yeni link oluşturuldu. LongUrl: {LongUrl}, ShortCode: {ShortCode}",
            requestDto.LongUrl,
            shortUrl.ShortCode);
            return ToDto(shortUrl);
        }

        public async Task<List<HourlyStatsDto>> GetHourlyStatsAsync(string code)
        {
            string cacheKey = $"stats:hourly:{code}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<HourlyStatsDto>>(cachedData);
            }

            var stats = await _repository.GetLast24HoursStatsAsync(code);

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(stats),
                options
                );

            return stats;
        }


    }
}
