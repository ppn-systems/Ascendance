// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Domain.Enums;

/// <summary>
/// Specifies the tile coordinate system or space used for position calculations.
/// </summary>
/// <remarks>
/// Clarifies whether coordinates are in tile units or pixel units to prevent conversion errors.
/// </remarks>
public enum TileCoordinateSpace
{
    /// <summary>
    /// Tile-based coordinates (grid indices). Integer values represent tile positions.
    /// </summary>
    /// <remarks>
    /// Example: (5, 3) means column 5, row 3 in the tile grid.
    /// </remarks>
    Tile = 0,

    /// <summary>
    /// World-based coordinates (pixel positions). Floating-point values in pixel space.
    /// </summary>
    /// <remarks>
    /// Example: (160.5, 96.0) means 160.5 pixels from left, 96 pixels from top.
    /// </remarks>
    World = 1
}
