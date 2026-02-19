namespace UrlMint.Infrastructure.ExternalServices.GoogleSafeBrowsing
{
    public class SafeBrowsingRequest
    {
        public ClientInfo Client { get; set; }
        public ThreatInfo ThreatInfo { get; set; }
    }

    public class ClientInfo
    {
        public string ClientId { get; set; } = "UrlMint";
        public string ClientVersion { get; set; } = "1.0.0";
    }

    public class ThreatInfo
    {
        // What threates are we looking for? (Malware, Phishing vb.)
        public string[] ThreatTypes { get; set; } = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE" };
        public string[] PlatformTypes { get; set; } = new[] { "ANY_PLATFORM" };
        public string[] ThreatEntryTypes { get; set; } = new[] { "URL" };
        public List<ThreatEntry> ThreatEntries { get; set; }
    }

    public class ThreatEntry
    {
        public string Url { get; set; }
    }

    public class SafeBrowsingResponse
    {
        //  If list is empty, the Url is clean. If it is full, a threat has been detected.
        public List<Match> Matches { get; set; }
    }

    public class Match
    {
        public string ThreatType { get; set; }
        public string PlatformType { get; set; }
        public string ThreatUrl { get; set; }
    }
}
