namespace UrlMint.Domain.DTO
{
    public class ShortUrlResponseDto
    {
        public string LongUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }
    }
}
