using System.Net;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using ShopMaster.Api.Tests.Infrastructure;

namespace ShopMaster.Api.Tests;

/// <summary>
/// PBI-04 : tests automatisés des endpoints de l'API.
/// Vérifient que l'API crée des comptes (POST /api/users) et les renvoie (GET /api/users, /api/users/{id}).
/// </summary>
public sealed class UsersApiTests(ShopMasterApiFactory factory) : IClassFixture<ShopMasterApiFactory>
{
    private async Task<UserJson> CreateAccountAsync(TestUser user)
    {
        var client = factory.CreateAuthenticatedClient(user.Token());
        var response = await client.PostAsync("/api/users", content: null);
        Assert.True(response.IsSuccessStatusCode, $"POST /api/users a renvoyé {(int)response.StatusCode}");
        return (await response.Content.ReadFromJsonAsync<UserJson>())!;
    }

    // ---------- Création de compte : POST /api/users ----------

    [Fact]
    public async Task CreateAccount_FirstLogin_Returns201WithCustomerRole()
    {
        var user = TestUser.Customer("Alice Martin");
        var client = factory.CreateAuthenticatedClient(user.Token());

        var response = await client.PostAsync("/api/users", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<UserJson>();
        Assert.NotNull(account);
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(user.Email, account.Email);
        Assert.Equal("Alice Martin", account.Name);
        Assert.Equal("Customer", account.Role);
        Assert.Equal($"/api/users/{account.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task CreateAccount_CalledTwice_Returns200WithSameAccount()
    {
        var user = TestUser.Customer();
        var first = await CreateAccountAsync(user);
        var client = factory.CreateAuthenticatedClient(user.Token());

        var response = await client.PostAsync("/api/users", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var second = await response.Content.ReadFromJsonAsync<UserJson>();
        Assert.Equal(first.Id, second!.Id);
    }

    [Fact]
    public async Task CreateAccount_WithAdminRoleClaim_StoresAdminRole()
    {
        var account = await CreateAccountAsync(TestUser.Admin());

        Assert.Equal("Admin", account.Role);
    }

    [Fact]
    public async Task CreateAccount_WhenRoleChangesInAuth0_SyncsRoleOnNextLogin()
    {
        var customer = TestUser.Customer();
        var created = await CreateAccountAsync(customer);

        var promoted = await CreateAccountAsync(customer with { Roles = ["Admin"] });

        Assert.Equal(created.Id, promoted.Id);
        Assert.Equal("Admin", promoted.Role);
    }

    [Fact]
    public async Task CreateAccount_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().PostAsync("/api/users", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Liste des comptes : GET /api/users ----------

    [Fact]
    public async Task GetUsers_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithTokenSignedByUnknownKey_Returns401()
    {
        var forgedKey = new SymmetricSecurityKey("a-key-that-auth0-never-issued-0123456789!"u8.ToArray());
        var token = TestJwt.CreateToken("auth0|forged", roles: ["Admin"], signingKey: forgedKey);

        var response = await factory.CreateAuthenticatedClient(token).GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithTokenForAnotherAudience_Returns401()
    {
        var token = TestJwt.CreateToken("auth0|other-api", roles: ["Admin"], audience: "https://another-api.test");

        var response = await factory.CreateAuthenticatedClient(token).GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_AsCustomer_Returns403()
    {
        var customer = TestUser.Customer();
        await CreateAccountAsync(customer);

        var response = await factory.CreateAuthenticatedClient(customer.Token()).GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsStoredAccounts()
    {
        var alice = await CreateAccountAsync(TestUser.Customer("Alice"));
        var bob = await CreateAccountAsync(TestUser.Customer("Bob"));
        var admin = TestUser.Admin();

        var response = await factory.CreateAuthenticatedClient(admin.Token()).GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<UserJson>>();
        Assert.Contains(accounts!, a => a.Id == alice.Id && a.Name == "Alice");
        Assert.Contains(accounts!, a => a.Id == bob.Id && a.Name == "Bob");
    }

    [Fact]
    public async Task GetUsers_DoesNotExposeAuth0Identifier()
    {
        var customer = TestUser.Customer();
        await CreateAccountAsync(customer);

        var json = await factory.CreateAuthenticatedClient(TestUser.Admin().Token()).GetStringAsync("/api/users");

        Assert.DoesNotContain(customer.Subject, json);
    }

    // ---------- Un compte : GET /api/users/{id} ----------

    [Fact]
    public async Task GetUserById_AsAdmin_ReturnsAccount()
    {
        var alice = await CreateAccountAsync(TestUser.Customer("Alice"));

        var response = await factory.CreateAuthenticatedClient(TestUser.Admin().Token())
            .GetAsync($"/api/users/{alice.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<UserJson>();
        Assert.Equal(alice, account);
    }

    [Fact]
    public async Task GetUserById_UnknownId_Returns404()
    {
        var response = await factory.CreateAuthenticatedClient(TestUser.Admin().Token())
            .GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_OwnAccountAsCustomer_Returns200()
    {
        var customer = TestUser.Customer();
        var account = await CreateAccountAsync(customer);

        var response = await factory.CreateAuthenticatedClient(customer.Token())
            .GetAsync($"/api/users/{account.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_OtherAccountAsCustomer_Returns403()
    {
        var alice = await CreateAccountAsync(TestUser.Customer("Alice"));
        var bob = TestUser.Customer("Bob");
        await CreateAccountAsync(bob);

        var response = await factory.CreateAuthenticatedClient(bob.Token())
            .GetAsync($"/api/users/{alice.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Mon compte : GET /api/users/me ----------

    [Fact]
    public async Task GetMe_AfterAccountCreation_ReturnsOwnAccount()
    {
        var customer = TestUser.Customer();
        var created = await CreateAccountAsync(customer);

        var response = await factory.CreateAuthenticatedClient(customer.Token()).GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created, await response.Content.ReadFromJsonAsync<UserJson>());
    }

    [Fact]
    public async Task GetMe_BeforeAccountCreation_Returns404()
    {
        var response = await factory.CreateAuthenticatedClient(TestUser.Customer().Token())
            .GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
