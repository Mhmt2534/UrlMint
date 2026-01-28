using HashidsNet;
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.Encoding
{
    public class UrlEncoder : IUrlEncoder
    {
        private readonly Hashids _hashids;

        public UrlEncoder(IConfiguration configuration)
        {
            var salt = configuration["HashidsSettings:Salt"];
            var minLegth = int.Parse(configuration["HashidsSettings:MinLength"]);

            _hashids = new Hashids(salt, minLegth);
        }

        public string Encode(long id)
        {
            //Convert the number to a hash (123 -> "X9a")
            return _hashids.EncodeLong(id);
        }



        public long Decode(string value)
        {
            try
            {
                //Convert the hash to the number ("X9a" -> 123)
                var decoded = _hashids.DecodeLong(value);

                if (decoded.Length == 0)
                {
                    throw new ArgumentException("Invalid Short Code");
                }

                return decoded[0];
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Short Code");
            }


        }
    }
}
