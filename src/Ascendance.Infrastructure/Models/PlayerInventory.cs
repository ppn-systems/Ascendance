// Copyright (c) 2026 Ascendance Team. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ascendance.Infrastructure.Models;

/// <summary>
/// Represents an item in player's inventory.
/// </summary>
[Table("player_inventory")]
public sealed class PlayerInventory
{
    /// <summary>
    /// Unique inventory entry ID.
    /// </summary>
    [Key]
    [Column("id")]
    public System.Int64 Id { get; set; }

    /// <summary>
    /// Player account ID (foreign key).
    /// </summary>
    [Column("player_id")]
    public System.Int64 PlayerId { get; set; }

    /// <summary>
    /// Item ID (reference to item definitions).
    /// </summary>
    [Column("item_id")]
    public System.Int32 ItemId { get; set; }

    /// <summary>
    /// Item quantity.
    /// </summary>
    [Column("quantity")]
    public System.Int32 Quantity { get; set; } = 1;

    /// <summary>
    /// Inventory slot index.
    /// </summary>
    [Column("slot_index")]
    public System.Int32 SlotIndex { get; set; }

    /// <summary>
    /// Item properties (enchantments, durability, etc.) stored as JSON.
    /// </summary>
    [Column("properties", TypeName = "jsonb")]
    public System.String Properties { get; set; }

    /// <summary>
    /// Timestamp when item was acquired.
    /// </summary>
    [Column("acquired_at")]
    public System.DateTime AcquiredAt { get; set; } = System.DateTime.UtcNow;

    /// <summary>
    /// Navigation property to player account.
    /// </summary>
    [ForeignKey(nameof(PlayerId))]
    public PlayerAccount Player { get; set; }
}