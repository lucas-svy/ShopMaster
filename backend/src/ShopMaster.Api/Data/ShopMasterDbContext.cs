using Microsoft.EntityFrameworkCore;
using ShopMaster.Api.Models;

namespace ShopMaster.Api.Data;

public class ShopMasterDbContext(DbContextOptions<ShopMasterDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
            user.Property(u => u.Auth0Id).HasColumnName("auth0_id").HasMaxLength(128);
            user.Property(u => u.Email).HasColumnName("email").HasMaxLength(320);
            user.Property(u => u.Name).HasColumnName("name").HasMaxLength(200);
            user.Property(u => u.Role).HasColumnName("role").HasMaxLength(32);
            user.Property(u => u.CreatedAt).HasColumnName("created_at");
            user.Property(u => u.LastLoginAt).HasColumnName("last_login_at");

            user.HasIndex(u => u.Auth0Id).IsUnique();
            user.HasIndex(u => u.Email);
        });
    }
}
