namespace UrlMint.Domain.Entities
{
    public class ClickLog
    {
        public long Id { get; set; }
        public string ShortCode { get; set; } 
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; } // Browser/OS Info
        public string? Referer { get; set; } // Which website did it come from?
        public string? Country { get; set; } // It will be found via IP address.
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    }
}
