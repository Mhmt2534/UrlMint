using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using UrlMint.Domain.DTO;
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

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy GetExecutionStrategy()
        {
            return  _context.Database.CreateExecutionStrategy();
        }


        public async Task<ShortUrl> CreateAsync(ShortUrl shortUrl)
        {
            _context.ShortUrls.Add(shortUrl);
            await _context.SaveChangesAsync();
            return shortUrl;
        }

        public async Task<bool> SaveChangesAsync()
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

        public async Task BatchUpdateClickCountAsync(string shortCode, long countToAdd)
        {
            // Direct update with SQL (The most efficient method)
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"ShortUrls\" SET \"ClickCount\" = \"ClickCount\" + {0} WHERE \"ShortCode\" = {1}",
                countToAdd, shortCode);
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

        public async Task<ShortUrl> GetByShortCodeAsync(string shortCode)
        {
            return await _context.ShortUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(x => shortCode == x.ShortCode);
        }


        public async Task CreatBatchAsync(IEnumerable<ShortUrl> shortUrls)
        {
            await _context.ShortUrls.AddRangeAsync(shortUrls);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOldExpiredUrlsAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(
                    @"
                    DELETE FROM ""ShortUrls""
                    WHERE ""ExpiresAt"" IS NOT NULL
                    AND ""ExpiresAt"" < NOW() - INTERVAL '30 days'
                    "
                );
        }

        public async Task<bool> ExistsAsync(string shortCode)
        {
            return await _context.ShortUrls.AnyAsync(
                x => x.ShortCode == shortCode
                );
        }


        public async Task<List<HourlyStatsDto>> GetLast24HoursStatsAsync(string shortCode)
        {
            // Bu SQL sorgusu şunu yapar:
            // 1. Son 24 saatin listesini oluşturur (generate_series).
            // 2. ClickLogs tablosuyla "LEFT JOIN" yapar.
            // 3. Veri olmayan saatleri "0" olarak sayar.

            FormattableString sql = $@"
        WITH hours AS (
            SELECT generate_series(
                date_trunc('hour', NOW()) - INTERVAL '23 hours',
                date_trunc('hour', NOW()),
                '1 hour'::interval
            ) as hour_start
        )
        SELECT 
            to_char(h.hour_start, 'HH24:00') as ""Hour"",
            COALESCE(COUNT(c.""Id""), 0) as ""Count""
        FROM hours h
        LEFT JOIN ""ClickLogs"" c 
            ON date_trunc('hour', c.""ClickedAt"") = h.hour_start 
            AND c.""ShortCode"" = {shortCode}
        GROUP BY h.hour_start
        ORDER BY h.hour_start ASC";

            return await _context.Database
                .SqlQuery<HourlyStatsDto>(sql)
                .ToListAsync();
        }
    }
}
