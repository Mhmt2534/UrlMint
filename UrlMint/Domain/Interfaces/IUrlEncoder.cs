namespace UrlMint.Domain.Interfaces
{
    public interface IUrlEncoder
    {
        string Encode(long id);
        long Decode(string value);
    }
}
