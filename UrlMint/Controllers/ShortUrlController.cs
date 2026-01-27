using Microsoft.AspNetCore.Mvc;
using UrlMint.Domain.DTO;
using UrlMint.Services.Interfaces;

namespace UrlMint.Controllers
{
    [Route("api/url")]
    [ApiController]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlService _service;

        public ShortUrlController(IShortUrlService service)
        {
            _service = service;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortUrlRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.LongUrl))
                return BadRequest(new { error = "URL is required." });

            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
                return BadRequest(new { error = "Invalid URL format." });

            var existing = await _service.GetByLongUrlAsync(request);
            if (existing!=null)
            {
                var response = CreateResponse(existing);
                return Ok(response);
            }

            var created = await _service.UrlShortener(request);
            var createdResponse = CreateResponse(created);

            return CreatedAtAction(
                nameof(GetUrlInfo),
                new { code = created.ShortCode },
               createdResponse
            );
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToLongUrl(string code)
        {
            if (code == "favicon.ico") return NotFound();
          
                var headers = Request.Headers;
                bool isPrefetch = headers.ContainsKey("sec-purpose") &&
                                  headers["sec-purpose"].ToString().ToLower().Contains("prefetch");
            try
            {
                // We are transferring the business logic to the service (Queue operation is inside the service)
                var longUrl = await _service.RedirectToLongUrl(code, isPrefetch);

                if (string.IsNullOrEmpty(longUrl))
                {
                    return NotFound(new { error = "The URL could not be found" });
                }

                // 3. We only provide redirect.
                return Redirect(longUrl);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = "Invalid short URL code." });
            }
        }

        [HttpGet("info/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            try
            {
                var shortUrl = await _service.GetByIdAsync(code);

                if (shortUrl == null)
                    return NotFound(new { error = "The URL could not be found" });

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
                return BadRequest(new { error = "Invalide short URL code" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUrls()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        
        //Helper Methods
        private object  CreateResponse(ShortUrlResponseDto dto)
        {
            return new
            {
                shortUrl = $"{Request.Scheme}://{Request.Host}/{dto.ShortCode}",
                shortCode = dto.ShortCode,
                longUrl = dto.LongUrl,
                createdAt = dto.CreatedAt 
            };
        }



    }
}

