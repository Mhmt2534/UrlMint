using Microsoft.AspNetCore.Mvc;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;

namespace UrlMint.Services.Interfaces
{
    public interface IShortUrlService
    {
        Task<IEnumerable<ShortUrlResponseDto>> GetAllAsync();

        Task<ShortUrlResponseDto> UrlShortener(ShortUrlRequestDto requestDto);

        Task<ShortUrlResponseDto> GetByLongUrlAsync(ShortUrlRequestDto requestDto);
        Task<ShortUrlResponseDto> GetByIdAsync(string code);
        Task<string> RedirectToLongUrl(string code, bool isPrefetch, ClickEventDto clickData);
        Task<ShortUrlResponseDto> GetByShortCodeAsync(string code);
        Task SeedDataAsync(List<string> longUrls);

    }
}
