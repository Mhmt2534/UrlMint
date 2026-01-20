using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;
using UrlMint.Services.Interfaces;

namespace UrlMint.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _repository;
        private readonly IUrlEncoder _encoder;
        private readonly IBackgroundTaskQueue _queue;

        public ShortUrlService(IShortUrlRepository repository, IUrlEncoder encoder, IBackgroundTaskQueue queue)
        {
            _repository = repository;
            _encoder = encoder;
            _queue = queue;
        }

        public async Task<IEnumerable<ShortUrlResponseDto>> GetAllAsync()
        {
            var urls = await _repository.GetAllAsync();
                
            var result =  urls.Select(u => new ShortUrlResponseDto
            {
                ShortCode = _encoder.Encode(u.Id),
                LongUrl = u.LongUrl,
                CreatedAt = u.CreatedAt,
                ClickCount = u.ClickCount
            });

            return result;
        }

        public async Task<ShortUrlResponseDto> GetByIdAsync(string code)
        {
            var id = _encoder.Decode(code);
            var response = await _repository.GetByIdAsync(id);
            return new ShortUrlResponseDto
            {
                ShortCode=code,
                LongUrl = response.LongUrl,
                CreatedAt = response.CreatedAt,
                ClickCount = response.ClickCount
            };
        }

        public async Task<string> RedirectToLongUrl(string code, bool isPrefetch)
        {
            var id = _encoder.Decode(code);
            var response = await _repository.GetByIdAsync(id);

            if (response == null) return null;

            if (!isPrefetch)
            {
                await _queue.QueueBackgroundWorkItemAsync(async (serviceProvider, token) =>
                {
                    var repo = serviceProvider.GetRequiredService<IShortUrlRepository>();

                    await repo.UpdateClickCountAsync(id);
                    Console.WriteLine($"Click count updated via Queue for ID: {id}");
                });

            }
            else
            {
                Console.WriteLine("Preload detected, counter not incremented");
            }

            return response.LongUrl;
        }


        public async Task<ShortUrlResponseDto> GetByLongUrlAsync(ShortUrlRequestDto requestDto)
        {

            var result = await _repository.GetByLongUrlAsync(requestDto.LongUrl);

            if (result == null)
            {
                return null;
            }

            var responseDto = new ShortUrlResponseDto
            {
                ShortCode = _encoder.Encode(result.Id),
                LongUrl = result.LongUrl,
                CreatedAt = result.CreatedAt,
                ClickCount = result.ClickCount
            };
            return responseDto;
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
            var shortCode = _encoder.Encode(created.Id);

            return new ShortUrlResponseDto
            {
                ShortCode = shortCode,
                LongUrl = created.LongUrl,
                CreatedAt = created.CreatedAt,
                ClickCount = 0
            };
        }
    }
}
