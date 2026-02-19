namespace UrlMint.Domain.Interfaces
{
    public interface IUrlSafetyService
    {
        Task<bool> IsUrlSafeAsync(string url);
    }
}
