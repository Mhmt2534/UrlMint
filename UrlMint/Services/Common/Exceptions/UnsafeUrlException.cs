namespace UrlMint.Services.Common.Exceptions
{
    public class UnsafeUrlException : Exception
    {
        public UnsafeUrlException(string message) : base(message) {}
    }
}
