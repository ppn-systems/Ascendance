// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Ascendance.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Ascendance.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the game.
/// </summary>
public sealed class GameDbContext : DbContext
{
    /// <summary>
    /// Player accounts table.
    /// </summary>
    public DbSet<PlayerAccount> PlayerAccounts { get; set; } = null!;

    /// <summary>
    /// Player inventory table.
    /// </summary>
    public DbSet<PlayerInventory> PlayerInventory { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameDbContext"/> class.
    /// </summary>
    [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    [RequiresUnreferencedCode("EF Core is not fully compatible with trimming. See https://aka.ms/efcore-docs-trimming")]
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the model relationships and constraints.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PlayerAccount configuration
        _ = modelBuilder.Entity<PlayerAccount>(entity =>
        {
            // Unique constraints
            _ = entity.HasIndex(e => e.Username).IsUnique();
            _ = entity.HasIndex(e => e.Email).IsUnique();

            // Indexes for performance
            _ = entity.HasIndex(e => e.Status);
            _ = entity.HasIndex(e => e.LastLoginAt);

            // One-to-many relationship with inventory
            _ = entity.HasMany(e => e.Inventory)
                    .WithOne(e => e.Player)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        // PlayerInventory configuration
        _ = modelBuilder.Entity<PlayerInventory>(entity =>
        {
            // Composite index for faster queries
            _ = entity.HasIndex(e => new { e.PlayerId, e.SlotIndex });
            _ = entity.HasIndex(e => e.ItemId);
        });
    }
}