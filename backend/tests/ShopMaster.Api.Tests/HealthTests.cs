using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ShopMaster.Api.Tests.Infrastructure;

namespace ShopMaster.Api.Tests;

public sealed class HealthTests(ShopMasterApiFactory factory) : IClassFixture<ShopMasterApiFactory>
{
    [Fact]
    public async Task Health_IsPublic_AndReturns200()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_WhenDatabaseReachable_Returns200()
    {
        var response = await factory.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void HealthChecks_AreBoundedByATimeout()
    {
        var options = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        Assert.NotEmpty(options.Registrations);
        Assert.All(options.Registrations, r => Assert.True(r.Timeout > TimeSpan.Zero && r.Timeout <= TimeSpan.FromSeconds(5),
            $"Le contrôle '{r.Name}' n'a pas de délai maximal : /health/ready peut rester bloqué si la base ne répond pas."));
    }
}
