// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Serialization;

namespace Ascendance.Shared.Maps;

/// <summary>
/// Represents a 2D tile-based map.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public class TileMap(System.Int32 width, System.Int32 height)
{
    #region Properties

    /// <summary>
    /// Map width (number of tiles).
    /// </summary>
    [SerializeOrder(0)]
    public System.Int32 Width { get; } = width;

    /// <summary>
    /// Map height (number of tiles).
    /// </summary>
    [SerializeOrder(1)]
    public System.Int32 Height { get; } = height;

    /// <summary>
    /// 2D tile array, accessed by [x, y].
    /// </summary>
    [SerializeOrder(2)]
    public Tile[,] Tiles { get; } = new Tile[width, height];

    #endregion Properties

    #region APIs

    /// <summary>
    /// Set a tile at (x, y).
    /// </summary>
    public void SetTile(System.Int32 x, System.Int32 y, Tile tile)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        Tiles[x, y] = tile;
    }

    /// <summary>
    /// Check if the tile at (x, y) is passable.
    /// </summary>
    public System.Boolean IsPassable(System.Int32 x, System.Int32 y)
    {
        Tile tile = GetTile(x, y);
        return tile?.IsPassable == true;
    }

    /// <summary>
    /// Kiểm tra một vị trí dạng pixel/float có nằm trong map không (nếu cần!)
    /// </summary>
    public System.Boolean Contains(System.Single worldX, System.Single worldY, System.Single tileSize)
    {
        System.Int32 tx = (System.Int32)(worldX / tileSize);
        System.Int32 ty = (System.Int32)(worldY / tileSize);
        return tx >= 0 && tx < Width && ty >= 0 && ty < Height;
    }

    /// <summary>
    /// Get the tile at (x, y). Returns null if out of bounds.
    /// </summary>
    public Tile GetTile(System.Int32 x, System.Int32 y) => x < 0 || x >= Width || y < 0 || y >= Height ? null : Tiles[x, y];

    #endregion APIs
}
