// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Enums;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Ascendance.Tiles;

/// <summary>
/// Provides static methods for collision detection between entities and tiles in a tile map.
/// </summary>
public static class TileCollider
{
    /// <summary>
    /// Checks if a rectangular bounds collides with any collidable tile in the specified layer.
    /// </summary>
    /// <param name="tileMap">The tile map to check collision against.</param>
    /// <param name="layerName">The name of the collision layer to query.</param>
    /// <param name="bounds">The entity bounds in world coordinates (pixels) to test for collision.</param>
    /// <returns><c>true</c> if the bounds intersect with at least one collidable tile; otherwise, <c>false</c>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean CheckCollision(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        if (tileMap is null || System.String.IsNullOrEmpty(layerName))
        {
            return false;
        }

        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer is null)
        {
            return false;
        }

        // Small epsilon to ensure the right/bottom edges are treated as inclusive for collision checking.
        const System.Single epsilon = 0.0001f;

        // Convert bounds to tile coordinates (inclusive)
        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(new Vector2f(bounds.Left + bounds.Width - epsilon, bounds.Top + bounds.Height - epsilon));

        // Clamp to valid tile range
        System.Int32 startX = System.Math.Max(0, topLeft.X);
        System.Int32 startY = System.Math.Max(0, topLeft.Y);
        System.Int32 endX = System.Math.Min(layer.Width - 1, bottomRight.X);
        System.Int32 endY = System.Math.Min(layer.Height - 1, bottomRight.Y);

        if (endX < startX || endY < startY)
        {
            return false;
        }

        // Check all tiles in the bounds
        for (System.Int32 y = startY; y <= endY; y++)
        {
            for (System.Int32 x = startX; x <= endX; x++)
            {
                ref readonly Tile tile = ref layer.GetTileRef(x, y);

                // Note: tile.IsEmpty() and tile.IsCollidable are cheap checks
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
    /// <returns>A list of collidable tiles that intersect with the bounds.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static List<TileInfo> GetCollidingTiles(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        List<TileInfo> result = [];

        if (tileMap is null || System.String.IsNullOrEmpty(layerName))
        {
            return result;
        }

        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer is null || !layer.Visible)
        {
            return result;
        }

        const System.Single epsilon = 0.0001f;

        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(new Vector2f(bounds.Left + bounds.Width - epsilon, bounds.Top + bounds.Height - epsilon));

        System.Int32 startX = System.Math.Max(0, topLeft.X);
        System.Int32 startY = System.Math.Max(0, topLeft.Y);
        System.Int32 endX = System.Math.Min(layer.Width - 1, bottomRight.X);
        System.Int32 endY = System.Math.Min(layer.Height - 1, bottomRight.Y);

        if (endX < startX || endY < startY)
        {
            return result;
        }

        for (System.Int32 y = startY; y <= endY; y++)
        {
            for (System.Int32 x = startX; x <= endX; x++)
            {
                ref readonly Tile tile = ref layer.GetTileRef(x, y);
                if (!tile.IsEmpty() && tile.IsCollidable)
                {
                    result.Add(new TileInfo(tile, (System.Int16)x, (System.Int16)y));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves a collision using the specified resolution strategy.
    /// </summary>
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

    private static Vector2f RESOLVE_PUSH(
        TileMap tileMap,
        System.String layerName,
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size)
    {
        Vector2f[] directions =
        [
            new Vector2f(0, -1), new Vector2f(1, 0), new Vector2f(0, 1), new Vector2f(-1, 0), // Cardinal
            new Vector2f(1, -1), new Vector2f(1, 1), new Vector2f(-1, 1), new Vector2f(-1, -1) // Diagonal
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
}