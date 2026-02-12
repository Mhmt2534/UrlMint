namespace UrlMint.Services.Common.Events
{
    public record ClickEvent(
         string ShortCode,
        string UserAgent,
        string IpHash,
        string? Referrer,
        DateTime ClickedAt
    );
}
