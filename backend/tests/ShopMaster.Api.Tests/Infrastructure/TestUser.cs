namespace ShopMaster.Api.Tests.Infrastructure;

/// <summary>Identité Auth0 fictive, unique à chaque test pour que les tests restent indépendants.</summary>
public sealed record TestUser(string Subject, string Email, string Name, string[] Roles)
{
    public static TestUser Customer(string name = "Client Test") => Create(name, []);

    public static TestUser Admin(string name = "Admin Test") => Create(name, ["Admin"]);

    private static TestUser Create(string name, string[] roles)
    {
        var id = Guid.NewGuid().ToString("N");
        return new TestUser($"auth0|{id}", $"{id}@shopmaster.test", name, roles);
    }

    public string Token() => TestJwt.CreateToken(Subject, Email, Name, Roles);
}

/// <summary>Contrat JSON attendu pour un compte renvoyé par l'API.</summary>
public sealed record UserJson(
    Guid Id,
    string? Email,
    string? Name,
    string Role,
    DateTime CreatedAt,
    DateTime LastLoginAt);
