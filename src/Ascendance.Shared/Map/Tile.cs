// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Enums;
using Nalix.Common.Serialization;

namespace Ascendance.Shared.Map;

/// <summary>
/// Represents a tile on a tile-based map.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public class Tile
{
    /// <summary>
    /// Type or ID of the tile (0: ground, 1: wall, ...).
    /// </summary>
    [SerializeOrder(0)]
    public TileType Type { get; set; }

    /// <summary>
    /// True if the tile can be passed through, false if blocked.
    /// </summary>
    [SerializeOrder(1)]
    public System.Boolean IsPassable { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Tile(TileType type, System.Boolean isPassable)
    {
        this.Type = type;
        this.IsPassable = isPassable;
    }
}
