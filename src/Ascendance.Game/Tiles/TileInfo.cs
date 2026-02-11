// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Game.Entities;

namespace Ascendance.Game.Tiles;

/// <summary>
/// Represents tile information including position in grid.
/// </summary>
/// <param name="Tile">The tile data.</param>
/// <param name="X">The X coordinate in tile grid.</param>
/// <param name="Y">The Y coordinate in tile grid.</param>
public readonly record struct TileInfo(Tile Tile, System.Int16 X, System.Int16 Y);
