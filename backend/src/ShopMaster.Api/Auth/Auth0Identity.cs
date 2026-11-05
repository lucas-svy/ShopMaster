using System.Security.Claims;
using ShopMaster.Api.Models;

namespace ShopMaster.Api.Auth;

/// <summary>Ce que l'API sait de l'utilisateur connecté, d'après son jeton Auth0.</summary>
public sealed record Auth0Identity(string Subject, string? Email, string? Name, bool IsAdmin)
{
    public string Role => IsAdmin ? Roles.Admin : Roles.Customer;

    public static Auth0Identity? FromPrincipal(ClaimsPrincipal principal, Auth0Options options)
    {
        var subject = principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(subject)) return null;

        return new Auth0Identity(
            subject,
            principal.FindFirstValue(options.EmailClaim) ?? principal.FindFirstValue("email"),
            principal.FindFirstValue(options.NameClaim) ?? principal.FindFirstValue("name"),
            principal.IsInRole(Roles.Admin));
    }
}
