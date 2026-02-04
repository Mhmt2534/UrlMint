using Microsoft.EntityFrameworkCore.Storage;
using UrlMint.Domain.Entities;

namespace UrlMint.Domain.Interfaces
{
    public interface IShortUrlRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        IExecutionStrategy GetExecutionStrategy();
        Task<ShortUrl> CreateAsync(ShortUrl shortUrl);
        Task<bool> UpdateAsync(ShortUrl shortUrl);  
        Task<ShortUrl> GetByIdAsync(long id);
        Task<ShortUrl> GetByShortCodeAsync(string shortCode);
        Task<ShortUrl> GetByLongUrlAsync(string longUrl);
        Task<bool> UpdateClickCountAsync(long id);
        Task BatchUpdateClickCountAsync(string shortCode, long countToAdd);
        Task<IEnumerable<ShortUrl>> GetAllAsync();

    }
}
