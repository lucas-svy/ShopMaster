using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ShopMaster.Api.Auth;
using ShopMaster.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Render impose le port d'écoute via la variable PORT (en local / Docker Compose : 8080).
if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// --- Base de données PostgreSQL (PBI-02) ---
builder.Services.AddDbContext<ShopMasterDbContext>((services, options) =>
{
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("Default")
        ?? throw new InvalidOperationException("La chaîne de connexion 'ConnectionStrings:Default' est manquante.");
    options.UseNpgsql(PostgresConnectionString.Normalize(connectionString),
        npgsql => npgsql.EnableRetryOnFailure());
});

// --- Authentification Auth0 : l'API valide les jetons JWT émis par Auth0 (PBI-03) ---
builder.Services.AddOptions<Auth0Options>()
    .BindConfiguration(Auth0Options.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<Auth0Options>>((jwt, auth0) => auth0.Value.ConfigureJwtBearer(jwt));
builder.Services.AddAuthorization();

// --- CORS : le frontend est servi depuis une autre origine (liste séparée par des virgules) ---
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins((builder.Configuration["Cors:AllowedOrigins"] ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<ShopMasterDbContext>(tags: ["ready"]);
// Base injoignable : répondre "Unhealthy" en 5 s au lieu d'attendre la fin des tentatives de reconnexion EF Core.
builder.Services.Configure<HealthCheckServiceOptions>(options =>
{
    foreach (var registration in options.Registrations) registration.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// Applique les migrations EF Core au démarrage (crée / met à jour la table "users").
if (app.Configuration.GetValue("Database:MigrateOnStartup", defaultValue: true))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ShopMasterDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// /health : le processus répond (utilisé par Render) ; /health/ready : la base est joignable.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapControllers();

app.Run();

/// <summary>Rendu visible pour les tests d'intégration (WebApplicationFactory).</summary>
public partial class Program;
