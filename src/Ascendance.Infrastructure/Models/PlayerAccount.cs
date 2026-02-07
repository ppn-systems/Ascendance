// Copyright (c) 2026 Ascendance Team. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Ascendance.Infrastructure.Models;

/// <summary>
/// Represents a player account in the database.
/// </summary>
[Table("player_accounts")]
public sealed class PlayerAccount
{
    /// <summary>
    /// Unique player account ID.
    /// </summary>
    [Key]
    [Column("id")]
    public System.Int64 Id { get; set; }

    /// <summary>
    /// Username (unique).
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Column("username")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public System.String Username { get; set; } = System.String.Empty;

    /// <summary>
    /// Email address (unique).
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("email")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public System.String Email { get; set; } = System.String.Empty;

    /// <summary>
    /// Hashed password (BCrypt or Argon2).
    /// </summary>
    [Required]
    [Column("password_hash")]
    public System.String PasswordHash { get; set; } = System.String.Empty;

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    [Column("created_at")]
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp.
    /// </summary>
    [Column("last_login_at")]
    public System.DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Account status (Active, Banned, Suspended).
    /// </summary>
    [Column("status")]
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    /// <summary>
    /// Ban expiration date (null if not banned).
    /// </summary>
    [Column("ban_expires_at")]
    public System.DateTime? BanExpiresAt { get; set; }

    /// <summary>
    /// Player level.
    /// </summary>
    [Column("level")]
    public System.Int32 Level { get; set; } = 1;

    /// <summary>
    /// Player experience points.
    /// </summary>
    [Column("experience")]
    public System.Int64 Experience { get; set; } = 0;

    /// <summary>
    /// In-game currency (gold).
    /// </summary>
    [Column("gold")]
    public System.Int64 Gold { get; set; } = 0;

    /// <summary>
    /// Premium currency (gems/diamonds).
    /// </summary>
    [Column("gems")]
    public System.Int32 Gems { get; set; } = 0;

    /// <summary>
    /// Last known position (stored as JSON).
    /// Example: {"x": 100.5, "y": 200.3, "z": 50.1, "map": "world_01"}
    /// </summary>
    [Column("last_position", TypeName = "jsonb")]
    public System.String LastPosition { get; set; }

    /// <summary>
    /// Player statistics (stored as JSON).
    /// </summary>
    [Column("statistics", TypeName = "jsonb")]
    public System.String Statistics { get; set; }

    /// <summary>
    /// Navigation property to player inventory.
    /// </summary>
    public System.Collections.Generic.List<PlayerInventory> Inventory { get; set; } = [];
}

/// <summary>
/// Account status enumeration.
/// </summary>
public enum AccountStatus : System.Byte
{
    /// <summary>
    /// Account is active and can log in.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Account is permanently banned.
    /// </summary>
    Banned = 1,

    /// <summary>
    /// Account is temporarily suspended.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Account is pending verification.
    /// </summary>
    PendingVerification = 3
}