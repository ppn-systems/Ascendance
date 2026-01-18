// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Shared.Enums;

/// <summary>
/// Represents the type of a tile within a tilemap.
/// Used to determine rendering, collision, and gameplay behavior.
/// </summary>
public enum TileType : System.Byte
{
    /// <summary>
    /// Unknown or undefined tile type.
    /// Should be avoided in finalized maps.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Empty tile with no collision.
    /// Typically used for open space or background.
    /// </summary>
    Air = 1,

    /// <summary>
    /// Solid ground tile.
    /// Entities can stand or walk on this tile.
    /// </summary>
    Ground = 2,

    /// <summary>
    /// Solid wall tile.
    /// Blocks movement horizontally and vertically.
    /// </summary>
    Wall = 3,

    /// <summary>
    /// Water tile.
    /// May affect movement speed, physics, or special states.
    /// </summary>
    Water = 4,

    /// <summary>
    /// Grass tile.
    /// Usually walkable ground with decorative or gameplay effects.
    /// </summary>
    Grass = 5
}
