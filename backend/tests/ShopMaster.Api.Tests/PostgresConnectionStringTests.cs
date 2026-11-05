using Npgsql;
using ShopMaster.Api.Data;

namespace ShopMaster.Api.Tests;

/// <summary>
/// Render fournit l'URL de la base au format "postgresql://user:pass@host:port/db",
/// que Npgsql ne comprend pas : l'API doit la convertir.
/// </summary>
public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_RenderUrl_IsConvertedToNpgsqlFormat()
    {
        var result = PostgresConnectionString.Normalize(
            "postgresql://shop_user:s3cr%40t@dpg-abc123-a:5433/shopmaster");

        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal("dpg-abc123-a", builder.Host);
        Assert.Equal(5433, builder.Port);
        Assert.Equal("shopmaster", builder.Database);
        Assert.Equal("shop_user", builder.Username);
        Assert.Equal("s3cr@t", builder.Password);
    }

    [Fact]
    public void Normalize_UrlWithoutPort_UsesDefaultPostgresPort()
    {
        var result = PostgresConnectionString.Normalize("postgres://u:p@db.example.com/app");

        Assert.Equal(5432, new NpgsqlConnectionStringBuilder(result).Port);
    }

    [Fact]
    public void Normalize_UrlWithSslModeRequire_KeepsSslMode()
    {
        var result = PostgresConnectionString.Normalize("postgresql://u:p@db.example.com/app?sslmode=require");

        Assert.Equal(SslMode.Require, new NpgsqlConnectionStringBuilder(result).SslMode);
    }

    [Fact]
    public void Normalize_KeyValueConnectionString_IsReturnedUnchanged()
    {
        const string connectionString = "Host=db;Port=5432;Database=shopmaster;Username=shopmaster;Password=pwd";

        Assert.Equal(connectionString, PostgresConnectionString.Normalize(connectionString));
    }
}
