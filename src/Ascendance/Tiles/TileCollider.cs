// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Enums;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Tiles;

/// <summary>
/// Provides static methods for collision detection between entities and tiles in a tile map.
/// </summary>
/// <remarks>
/// This utility class handles spatial queries and collision resolution for tile-based collision detection.
/// All methods are stateless and thread-safe for read-only tile map operations.
/// </remarks>
public static class TileCollider
{
    /// <summary>
    /// Checks if a rectangular bounds collides with any collidable tile in the specified layer.
    /// </summary>
    /// <param name="tileMap">The tile map to check collision against.</param>
    /// <param name="layerName">The name of the collision layer to query.</param>
    /// <param name="bounds">The entity bounds in world coordinates (pixels) to test for collision.</param>
    /// <returns>
    /// <c>true</c> if the bounds intersect with at least one collidable tile; otherwise, <c>false</c>.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean CheckCollision(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer is null)
        {
            return false;
        }

        // Convert bounds to tile coordinates
        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(new Vector2f(bounds.Left + bounds.Width, bounds.Top + bounds.Height));

        // Clamp to valid tile range
        System.Int32 startX = System.Math.Max(0, topLeft.X);
        System.Int32 startY = System.Math.Max(0, topLeft.Y);
        System.Int32 endX = System.Math.Min(layer.Width - 1, bottomRight.X);
        System.Int32 endY = System.Math.Min(layer.Height - 1, bottomRight.Y);

        // Check all tiles in the bounds
        for (System.Int32 y = startY; y <= endY; y++)
        {
            for (System.Int32 x = startX; x <= endX; x++)
            {
                ref readonly Tile tile = ref layer.GetTileRef(x, y);
                if (!tile.IsEmpty() && tile.IsCollidable)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Retrieves all collidable tiles that overlap with the specified bounds.
    /// </summary>
    /// <param name="tileMap">The tile map to query.</param>
    /// <param name="layerName">The name of the collision layer to search.</param>
    /// <param name="bounds">The rectangular area in world coordinates (pixels) to check.</param>
    /// <returns>A span of collidable tiles that intersect with the bounds.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<TileInfo> GetCollidingTiles(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        System.Collections.Generic.List<TileInfo> result = [];

        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer?.Visible != true)
        {
            return result;
        }

        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(
            new Vector2f(bounds.Left + bounds.Width, bounds.Top + bounds.Height));

        System.Int16 startX = (System.Int16)System.Math.Max(0, topLeft.X);
        System.Int16 startY = (System.Int16)System.Math.Max(0, topLeft.Y);
        System.Int16 endX = (System.Int16)System.Math.Min(layer.Width - 1, bottomRight.X);
        System.Int16 endY = (System.Int16)System.Math.Min(layer.Height - 1, bottomRight.Y);

        for (System.Int16 y = startY; y <= endY; y++)
        {
            for (System.Int16 x = startX; x <= endX; x++)
            {
                ref readonly Tile tile = ref layer.GetTileRef(x, y);
                if (!tile.IsEmpty() && tile.IsCollidable)
                {
                    result.Add(new TileInfo(tile, x, y));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves a collision using the specified resolution strategy.
    /// </summary>
    /// <param name="tileMap">The tile map to perform collision resolution against.</param>
    /// <param name="layerName">The name of the collision layer to use.</param>
    /// <param name="currentPos">The current valid position of the entity.</param>
    /// <param name="targetPos">The desired target position.</param>
    /// <param name="size">The size of the entity's bounding box.</param>
    /// <param name="mode">The collision resolution strategy to apply.</param>
    /// <returns>A resolved position based on the selected collision resolution mode.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Vector2f ResolveCollision(
        TileMap tileMap,
        System.String layerName,
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size,
        CollisionMode mode = CollisionMode.Slide)
    {
        return mode switch
        {
            CollisionMode.None => targetPos,
            CollisionMode.Stop => RESOLVE_STOP(tileMap, layerName, currentPos, targetPos, size),
            CollisionMode.Push => RESOLVE_PUSH(tileMap, layerName, currentPos, targetPos, size),
            CollisionMode.Slide => RESOLVE_SLIDE(tileMap, layerName, currentPos, targetPos, size),
            _ => currentPos
        };
    }

    #region Private Resolution Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector2f RESOLVE_STOP(
        TileMap tileMap,
        System.String layerName,
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size)
    {
        FloatRect targetBounds = new(targetPos.X, targetPos.Y, size.X, size.Y);
        return CheckCollision(tileMap, layerName, targetBounds) ? currentPos : targetPos;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector2f RESOLVE_SLIDE(
        TileMap tileMap,
        System.String layerName,
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size)
    {
        // Try X movement only
        FloatRect xBounds = new(targetPos.X, currentPos.Y, size.X, size.Y);
        if (!CheckCollision(tileMap, layerName, xBounds))
        {
            return new Vector2f(targetPos.X, currentPos.Y);
        }

        // Try Y movement only
        FloatRect yBounds = new(currentPos.X, targetPos.Y, size.X, size.Y);
        if (!CheckCollision(tileMap, layerName, yBounds))
        {
            return new Vector2f(currentPos.X, targetPos.Y);
        }

        // Both axes collide
        return currentPos;
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector2f RESOLVE_PUSH(
        TileMap tileMap,
        System.String layerName,
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size)
    {
        System.ReadOnlySpan<Vector2f> directions =
        [
            new(0, -1), new(1, 0), new(0, 1), new(-1, 0),  // Cardinal
            new(1, -1), new(1, 1), new(-1, 1), new(-1, -1) // Diagonal
        ];

        const System.Single pushDistance = 2f;

        foreach (Vector2f dir in directions)
        {
            Vector2f testPos = new(
                targetPos.X + (dir.X * pushDistance),
                targetPos.Y + (dir.Y * pushDistance));

            FloatRect testBounds = new(testPos.X, testPos.Y, size.X, size.Y);

            if (!CheckCollision(tileMap, layerName, testBounds))
            {
                return testPos;
            }
        }

        return currentPos;
    }

    #endregion Private Resolution Methods
}