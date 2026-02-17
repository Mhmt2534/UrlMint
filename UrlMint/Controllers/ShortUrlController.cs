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

            return CreatedAtAction(
                nameof(GetUrlInfo), //action
                new { code = created.ShortCode }, //route value { "code" : 123}
               CreateResponse(created)
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

                var clickData = new ClickEventDto
                {
                    ShortCode = code,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Referer = Request.Headers["Referer"].ToString(),
                    Timestamp = DateTime.UtcNow
                };

                // We are transferring the business logic to the service (Queue operation is inside the service)
                var longUrl = await _service.RedirectToLongUrl(code, isPrefetch,clickData);

                if (string.IsNullOrEmpty(longUrl))
                {
                    return StatusCode(410,"Link expired or not found");
                }

                // 3. We only provide redirect.
                return Redirect(longUrl);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { error = "Invalid short URL code." });
            }
        }

        [HttpGet("analytics/{code}/hourly")]
        public async Task<IActionResult> GetHourlyStats(string code)
        {
            var stats = await _service.GetHourlyStatsAsync(code);
            return Ok(stats);
        }

        [HttpGet("info/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            try
            {
                var shortUrl = await _service.GetByIdAsync(code);

                if (shortUrl == null)
                    return NotFound(new { error = "The URL could not be found" });

                var response = CreateResponse(shortUrl);

                return Ok(response);
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


        [HttpGet("seed")]
        public async Task<IActionResult> AddSeeds()
        {
            var testUrls = new List<string>();
            for (int i = 0; i <= 10_000; i++)
            {
                testUrls.Add($"https://example.com/page-{i}");
            }

            await _service.SeedDataAsync(testUrls);
            return Ok("10.000 records have been successfully added.");
        }

        
        //Helper Methods
        private object  CreateResponse(ShortUrlResponseDto dto)
        {
            return new
            {
                shortUrl = $"{Request.Scheme}://{Request.Host}/{dto.ShortCode}",
                shortCode = dto.ShortCode,
                longUrl = dto.LongUrl,
                createdAt = dto.CreatedAt ,
                expriesAt = dto.ExpiresAt 
            };
        }

    }
}

