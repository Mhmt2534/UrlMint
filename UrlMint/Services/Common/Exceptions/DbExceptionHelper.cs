using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace UrlMint.Services.Common.Exceptions
{
    public static class DbExceptionHelper
    {
        public static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pg
                && pg.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
