using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ShopMaster.Api.Auth;

/// <summary>
/// Paramètres Auth0 (section "Auth0" de la configuration, ou variables Auth0__Domain, Auth0__Audience...).
/// </summary>
public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    /// <summary>Domaine du tenant Auth0, ex. "shopmaster.eu.auth0.com".</summary>
    [Required]
    public string Domain { get; set; } = "";

    /// <summary>Identifiant de l'API déclarée dans Auth0 (audience des jetons), ex. "https://api.shopmaster.app".</summary>
    [Required]
    public string Audience { get; set; } = "";

    /// <summary>Préfixe des claims personnalisés ajoutés par l'Action Auth0 (voir docs/auth0.md).</summary>
    public string ClaimsNamespace { get; set; } = "https://shopmaster.app";

    public string Authority =>
        Domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? Domain.TrimEnd('/') + "/"
            : $"https://{Domain.TrimEnd('/')}/";

    public string RolesClaim => $"{ClaimsNamespace.TrimEnd('/')}/roles";
    public string EmailClaim => $"{ClaimsNamespace.TrimEnd('/')}/email";
    public string NameClaim => $"{ClaimsNamespace.TrimEnd('/')}/name";

    public void ConfigureJwtBearer(JwtBearerOptions jwt)
    {
        jwt.Authority = Authority;
        jwt.Audience = Audience;
        // Garde les noms de claims tels qu'émis par Auth0 ("sub", claims namespacés...).
        jwt.MapInboundClaims = false;
        jwt.TokenValidationParameters.ValidIssuer = Authority;
        jwt.TokenValidationParameters.ValidAudience = Audience;
        jwt.TokenValidationParameters.NameClaimType = "sub";
        jwt.TokenValidationParameters.RoleClaimType = RolesClaim;
    }
}
