using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace ShopMaster.Api.Data;

public static class PostgresConnectionString
{
    /// <summary>
    /// Accepte une chaîne Npgsql classique ("Host=...;Database=...") ou une URL
    /// "postgresql://user:password@host:port/database" (format fourni par Render) et renvoie le format Npgsql.
    /// </summary>
    public static string Normalize(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("postgres" or "postgresql"))
        {
            return connectionString;
        }

        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
        };

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("sslmode", out var sslMode)
            && Enum.TryParse<SslMode>(sslMode.ToString().Replace("-", ""), ignoreCase: true, out var mode))
        {
            builder.SslMode = mode;
        }

        return builder.ConnectionString;
    }
}
