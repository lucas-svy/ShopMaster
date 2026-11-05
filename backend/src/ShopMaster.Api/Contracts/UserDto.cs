using System.Linq.Expressions;
using ShopMaster.Api.Models;

namespace ShopMaster.Api.Contracts;

/// <summary>Compte tel qu'exposé par l'API (l'identifiant Auth0 reste interne).</summary>
public sealed record UserDto(
    Guid Id,
    string? Email,
    string? Name,
    string Role,
    DateTime CreatedAt,
    DateTime LastLoginAt)
{
    /// <summary>Projection LINQ traduite en SQL par EF Core (seules ces colonnes sont lues).</summary>
    public static readonly Expression<Func<User, UserDto>> Projection =
        u => new UserDto(u.Id, u.Email, u.Name, u.Role, u.CreatedAt, u.LastLoginAt);

    private static readonly Func<User, UserDto> Map = Projection.Compile();

    public static UserDto From(User user) => Map(user);
}
