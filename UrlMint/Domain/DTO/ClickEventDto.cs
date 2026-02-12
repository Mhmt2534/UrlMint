namespace UrlMint.Domain.DTO
{
    public class ClickEventDto
    {
        public string ShortCode { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Referer { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
