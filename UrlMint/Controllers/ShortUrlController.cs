using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrlMint.Domain.DTO;
using UrlMint.Domain.Entities;
using UrlMint.Domain.Interfaces;

namespace UrlMint.Controllers
{
    [Route("api/url")]
    [ApiController]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlRepository _repository;
        private readonly IUrlEncoder _encoder;

        public ShortUrlController(IShortUrlRepository repository, IUrlEncoder encoder)
        {
            _repository = repository;
            _encoder = encoder;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LongUrl))
                return BadRequest(new { error = "URL gereklidir" });

            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
                return BadRequest(new { error = "Geçersiz URL formatı." });

            var existing = await _repository.GetByLongUrlAsync(request.LongUrl);
            if (existing!=null)
            {
                var existingCode = _encoder.Encode(existing.Id);
                return Ok(new
                {
                    shortUrl = $"{Request.Scheme}://{Request.Host}/{existingCode}",
                    shortCode = existingCode,
                    longUrl = existing.LongUrl,
                    createdAt = existing.CreatedAt
                });
            }

            var shortUrl = new ShortUrl
            {
                LongUrl = request.LongUrl,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };

            var created = await _repository.CreateAsync(shortUrl);
            var shortCode = _encoder.Encode(created.Id);

            return CreatedAtAction(
                nameof(GetUrlInfo),
                new { code = shortCode },
                new
                {
                    shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}",
                    shortCode = shortCode,
                    longUrl = created.LongUrl,
                    createdAt = created.CreatedAt
                });


        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToLongUrl(string code)
        {
            try
            {
                var id = _encoder.Decode(code);
                var shortUrl = await _repository.GetByIdAsync(id);

                if (shortUrl == null)
                    return NotFound(new { error = "URL bulunamadı." });

                // Click count'u artır (fire-and-forget)
                _ = _repository.UpdateClickCountAsync(id);

                return Redirect(shortUrl.LongUrl);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = "Geçersiz kısa URL kodu." });
            }
        }

        [HttpGet("info/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            try
            {
                var id = _encoder.Decode(code);
                var shortUrl = await _repository.GetByIdAsync(id);

                if (shortUrl == null)
                    return NotFound(new { error = "URL bulunamadı." });

                return Ok(new
                {
                    shortCode = code,
                    longUrl = shortUrl.LongUrl,
                    createdAt = shortUrl.CreatedAt,
                    clickCount = shortUrl.ClickCount
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = "Geçersiz kısa URL kodu." });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUrls()
        {
            var urls = await _repository.GetAllAsync();
            var result = urls.Select(u => new
            {
                shortCode = _encoder.Encode(u.Id),
                longUrl = u.LongUrl,
                createdAt = u.CreatedAt,
                clickCount = u.ClickCount
            });

            return Ok(result);
        }
    }

}

