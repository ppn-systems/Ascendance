// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Contracts.Enums;

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
    /// Grass tile.
    /// Usually walkable ground with decorative or gameplay effects.
    /// </summary>
    Grass = 1,

    /// <summary>
    /// Dirt tile.
    /// </summary>
    Dirt = 2
}
