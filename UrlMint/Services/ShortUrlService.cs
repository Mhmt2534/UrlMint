using StackExchange.Redis; // Bu namespace gerekli
using Microsoft.Extensions.Caching.Distributed;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;
using UrlMint.Services.Interfaces;
using UrlMint.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UrlMint.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _repository;
        private readonly IUrlEncoder _encoder;
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redisDb; //Just redis
        private readonly UrlMintDbContext _dbContext;

        public ShortUrlService(IShortUrlRepository repository, IUrlEncoder encoder
            , IDistributedCache cache, IConnectionMultiplexer redisMultiplexer
            ,UrlMintDbContext dbContext)
        {
            _repository = repository;
            _encoder = encoder;
            _cache = cache;
            _redisDb = redisMultiplexer.GetDatabase();
            _dbContext = dbContext;
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

        public async Task<string> RedirectToLongUrl(string code, bool isPrefetch)
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
            }
            else
            {
                Console.WriteLine("Preload detected, counter not incremented");
            }

            return cachedUrl;

        }


        public async Task<ShortUrlResponseDto> GetByLongUrlAsync(ShortUrlRequestDto requestDto)
        {
            var entity = await _repository.GetByLongUrlAsync(requestDto.LongUrl);

            return ToDtoWithEncodedCode(entity);
        }


        public async Task<ShortUrlResponseDto> UrlShortener(ShortUrlRequestDto requestDto)
        {
            var shortUrl = new ShortUrl
            {
                LongUrl = requestDto.LongUrl,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };

            var created = await _repository.CreateAsync(shortUrl);

            created.ShortCode = _encoder.Encode(created.Id);

            await _repository.SaveChangesAsync();

            return ToDto(shortUrl);
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




    }
}
