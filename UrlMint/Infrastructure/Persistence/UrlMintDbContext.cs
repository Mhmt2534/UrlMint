using Microsoft.EntityFrameworkCore;
using UrlMint.Domain.Entities;

namespace UrlMint.Infrastructure.Persistence
{
    public class UrlMintDbContext : DbContext
    {
        public UrlMintDbContext(DbContextOptions<UrlMintDbContext> options) : base(options) { }

        public DbSet<ShortUrl> ShortUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortUrl>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x=>x.LongUrl).IsRequired();

                entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            });


        }

    }
}
