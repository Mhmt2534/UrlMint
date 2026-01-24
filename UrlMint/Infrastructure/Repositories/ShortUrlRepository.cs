using Microsoft.EntityFrameworkCore;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;
using UrlMint.Infrastructure.Persistence;

namespace UrlMint.Infrastructure.Repositories
{
    public class ShortUrlRepository : IShortUrlRepository
    {
        private readonly UrlMintDbContext _context;
        public ShortUrlRepository(UrlMintDbContext context)
        {
            _context = context;
        }

        public async Task<ShortUrl> CreateAsync(ShortUrl shortUrl)
        {
            _context.ShortUrls.Add(shortUrl);
            await _context.SaveChangesAsync();
            return shortUrl;
        }

        public async Task<bool> UpdateAsync(ShortUrl shortUrl)
        {
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UpdateClickCountAsync(long id)
        {
            var shortUrl = await _context.ShortUrls.FindAsync(id);
            if (shortUrl == null)
                return false;

            shortUrl.ClickCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ShortUrl>> GetAllAsync()
        {
            return await _context.ShortUrls
                .AsNoTracking()
                .OrderByDescending(x=>x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ShortUrl> GetByIdAsync(long id)
        {
            return await _context.ShortUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ShortUrl> GetByLongUrlAsync(string longUrl)
        {
            return await _context.ShortUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LongUrl == longUrl);
        }

       
       
    }
}
