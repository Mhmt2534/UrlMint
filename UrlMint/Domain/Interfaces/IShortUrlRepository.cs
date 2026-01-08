using UrlMint.Domain.Entities;

namespace UrlMint.Domain.Interfaces
{
    public interface IShortUrlRepository
    {
        Task<ShortUrl> CreateAsync(ShortUrl shortUrl);
        Task<ShortUrl> GetByIdAsync(long id);
        Task<ShortUrl> GetByLongUrlAsync(string longUrl);
        Task<bool> UpdateClickCountAsync(long id);
        Task<IEnumerable<ShortUrl>> GetAllAsync();

    }
}
