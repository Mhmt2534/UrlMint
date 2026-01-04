using System.Text;
using UrlMint.Domain.Interfaces;

namespace UrlMint.Infrastructure.Encoding
{
    public class Base62Encoder : IUrlEncoder
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Base = 62;

        public string Encode(long id)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id),"ID cannot be negative");

            if (id == Base)
                return Alphabet[0].ToString();

            var sb = new StringBuilder(); //String e gore daha az maliyetli, surekli yeni string yaratmaz.

            while (id > 0) 
            {
                int deneme=(int) id%Base;
                sb.Insert(0, Alphabet[deneme]);//0. index e Alphabet[deneme] eklenir.
                id /= Base;
            }

            return sb.ToString();
        }



        public long Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Code cannot be null or empty.",nameof(value));

            long result = 0;

            foreach (var v in value)
            {
                var index = Alphabet.IndexOf(v);
                if (index < 0)
                    throw new ArgumentException($"Invalid character {v} in Base62Encoder code");
                result = result*Base + index;
            }

            return result;

        }
    }
}
