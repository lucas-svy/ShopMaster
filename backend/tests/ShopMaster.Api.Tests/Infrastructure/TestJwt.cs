using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ShopMaster.Api.Tests.Infrastructure;

/// <summary>
/// Fabrique des jetons JWT au format Auth0 (claims "namespacés"), signés avec une clé de test.
/// L'API valide ces jetons avec le vrai pipeline JwtBearer : seule la clé de signature change.
/// </summary>
public static class TestJwt
{
    public const string Domain = "shopmaster-test.eu.auth0.com";
    public const string Issuer = $"https://{Domain}/";
    public const string Audience = "https://api.shopmaster.test";
    public const string ClaimsNamespace = "https://shopmaster.app";

    public static readonly SymmetricSecurityKey SigningKey =
        new(Encoding.UTF8.GetBytes("shopmaster-tests-signing-key-0123456789-abcdef!"));

    public static string CreateToken(
        string subject,
        string? email = null,
        string? name = null,
        string[]? roles = null,
        SecurityKey? signingKey = null,
        string audience = Audience)
    {
        var claims = new Dictionary<string, object> { ["sub"] = subject };
        if (email is not null) claims[$"{ClaimsNamespace}/email"] = email;
        if (name is not null) claims[$"{ClaimsNamespace}/name"] = name;
        if (roles is not null) claims[$"{ClaimsNamespace}/roles"] = roles;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Claims = claims,
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(signingKey ?? SigningKey, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
