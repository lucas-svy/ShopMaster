using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ShopMaster.Api.Data;

namespace ShopMaster.Api.Tests.Infrastructure;

/// <summary>
/// Démarre l'API en mémoire pour les tests :
/// - PostgreSQL est remplacé par une base SQLite en mémoire (aucun Docker requis) ;
/// - Auth0 est remplacé par une clé de signature locale (voir <see cref="TestJwt"/>).
/// </summary>
public sealed class ShopMasterApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ShopMasterApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", "Host=not-used-in-tests");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Auth0:Domain", TestJwt.Domain);
        builder.UseSetting("Auth0:Audience", TestJwt.Audience);
        builder.UseSetting("Auth0:ClaimsNamespace", TestJwt.ClaimsNamespace);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ShopMasterDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ShopMasterDbContext>>();
            services.AddDbContext<ShopMasterDbContext>(options => options.UseSqlite(_connection));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Pas d'appel réseau vers Auth0 : on valide la signature avec la clé de test.
                options.ConfigurationManager = null;
                options.Configuration = null;
                options.TokenValidationParameters.IssuerSigningKey = TestJwt.SigningKey;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ShopMasterDbContext>().Database.EnsureCreated();
        return host;
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
