namespace UrlMint.Domain.DTO
{
    public class ShortUrlRequestDto
    {
        public string LongUrl { get; set; } = string.Empty; 
        public string? CustomAlias { get; set; } = string.Empty;
    }
}
