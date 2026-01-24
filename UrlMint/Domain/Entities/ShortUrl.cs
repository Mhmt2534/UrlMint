namespace UrlMint.Domain.Entities
{
    public class ShortUrl
    {
        public long Id { get; set; }
        public string LongUrl { get; set; }
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }
    }
}
