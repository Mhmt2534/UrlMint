using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UrlMint.Services.Common.Validation
{
    public static class AliasValidator
    {
        private static readonly Regex regex =
            new("^[a-z0-9-]{3,30}$", RegexOptions.Compiled);


        private static readonly HashSet<string> Reserved =
            [
                "api","admin","health","swagger","docs"
            ];

        public static void Validate(string alias)
        {
            if (!regex.IsMatch(alias))
                throw new ValidationException("Invalid alias format");

            if (Reserved.Contains(alias))
                throw new ValidationException("Alias is reserved");
        }


    }
}
