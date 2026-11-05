using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopMaster.Api.Auth;
using ShopMaster.Api.Contracts;
using ShopMaster.Api.Data;
using ShopMaster.Api.Models;

namespace ShopMaster.Api.Controllers;

/// <summary>
/// Comptes utilisateurs (PBI-02 / PBI-03).
/// Toutes les routes exigent un jeton Auth0 valide.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    ShopMasterDbContext db,
    IOptions<Auth0Options> auth0Options,
    TimeProvider clock) : ControllerBase
{
    /// <summary>Liste de tous les comptes (administrateurs uniquement).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IReadOnlyList<UserDto>> GetAll(CancellationToken cancellationToken) =>
        await db.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .Select(UserDto.Projection)
            .ToListAsync(cancellationToken);

    /// <summary>Compte de l'utilisateur connecté.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        if (CurrentIdentity() is not { } identity) return Unauthorized();

        var account = await db.Users
            .AsNoTracking()
            .Where(u => u.Auth0Id == identity.Subject)
            .Select(UserDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return account is null
            ? Problem(statusCode: StatusCodes.Status404NotFound,
                detail: "Aucun compte pour cet utilisateur : appelez d'abord POST /api/users.")
            : account;
    }

    /// <summary>Un compte : l'utilisateur lui-même, ou n'importe lequel pour un administrateur.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentIdentity() is not { } identity) return Unauthorized();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        // Un client ne voit que son propre compte (403 sans révéler si l'id existe).
        if (!identity.IsAdmin && user?.Auth0Id != identity.Subject) return Forbid();

        return user is null ? NotFound() : UserDto.From(user);
    }

    /// <summary>
    /// Crée le compte de l'utilisateur connecté à sa première connexion (201),
    /// puis le resynchronise avec Auth0 (email, nom, rôle) aux connexions suivantes (200).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateOrSync(CancellationToken cancellationToken)
    {
        if (CurrentIdentity() is not { } identity) return Unauthorized();

        var now = clock.GetUtcNow().UtcDateTime;
        var user = await db.Users.FirstOrDefaultAsync(u => u.Auth0Id == identity.Subject, cancellationToken);

        if (user is not null)
        {
            SyncFromAuth0(user, identity, now);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(UserDto.From(user));
        }

        user = new User
        {
            Auth0Id = identity.Subject,
            Email = identity.Email,
            Name = identity.Name,
            Role = identity.Role,
            CreatedAt = now,
            LastLoginAt = now,
        };
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Deux premières requêtes simultanées : l'autre a déjà créé le compte (index unique sur auth0_id).
            db.ChangeTracker.Clear();
            var existing = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Auth0Id == identity.Subject, cancellationToken);
            if (existing is null) throw;
            return Ok(UserDto.From(existing));
        }

        return Created($"/api/users/{user.Id}", UserDto.From(user));
    }

    private Auth0Identity? CurrentIdentity() => Auth0Identity.FromPrincipal(User, auth0Options.Value);

    private static void SyncFromAuth0(User user, Auth0Identity identity, DateTime now)
    {
        user.Email = identity.Email ?? user.Email;
        user.Name = identity.Name ?? user.Name;
        user.Role = identity.Role;
        user.LastLoginAt = now;
    }
}
