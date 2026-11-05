using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopMaster.Api.Data;

/// <summary>
/// Utilisé uniquement par "dotnet ef migrations add ..." : génère les migrations PostgreSQL
/// sans avoir besoin d'une base démarrée ni de la configuration Auth0.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShopMasterDbContext>
{
    public ShopMasterDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ShopMasterDbContext>()
            .UseNpgsql("Host=localhost;Database=shopmaster_design_time")
            .Options);
}
