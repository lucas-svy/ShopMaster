namespace ShopMaster.Api.Models;

/// <summary>
/// Compte utilisateur ShopMaster (table "users").
/// L'identité (mot de passe, inscription, connexion) est gérée par Auth0 ;
/// la base ne stocke que le lien vers l'identité Auth0 et les données utiles à l'application.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>Identifiant Auth0 (claim "sub"), ex. "auth0|65f...". Unique.</summary>
    public required string Auth0Id { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string Role { get; set; } = Roles.Customer;

    public DateTime CreatedAt { get; set; }

    public DateTime LastLoginAt { get; set; }
}
