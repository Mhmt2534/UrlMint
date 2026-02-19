using System.Text.Json;
using System.Text;
using UrlMint.Domain.Interfaces;
using UrlMint.Infrastructure.ExternalServices.GoogleSafeBrowsing;
using Microsoft.Extensions.Configuration;

namespace UrlMint.Infrastructure.ExternalServices
{
    public class UrlSafetyService : IUrlSafetyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public UrlSafetyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["SafeBrowsing:ApiKey"];
            _apiUrl = $"{configuration["SafeBrowsing:BaseUrl"]}?key={_apiKey}";
        }

        public async Task<bool> IsUrlSafeAsync(string url)
        {
            var requestPayload = new SafeBrowsingRequest
            {
                Client = new ClientInfo(),
                ThreatInfo = new ThreatInfo
                {
                    ThreatEntries = new List<ThreatEntry> { new ThreatEntry { Url = url } }
                }
            };

            var json = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SafeBrowsingResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // If Matches is null or empty, the URL is clean.
                // If it's full, Google has detected a threat.
                return result?.Matches == null || result.Matches.Count == 0;
            }
            catch (Exception ex)
            {
                // What should I do if thee Google API doesn't work
                // "Fail Open" or "Fail Closed" ?
                // It is usually logged and temporarily allowed (to avoid stopping the system).I did the same.
                Console.WriteLine($"Safety check failed: {ex.Message}");
                return true; // Let's assume it's safe for now
            }
        }
    }
}
