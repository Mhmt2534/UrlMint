using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;
using UrlMint.Services.Interfaces;

namespace UrlMint.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepository _repository;
        public ShortUrlService(IShortUrlRepository repository)
        {
            _repository = repository;
        }

      
    }
}
