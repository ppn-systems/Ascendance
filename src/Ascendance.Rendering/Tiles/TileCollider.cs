// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tiles;

/// <summary>
/// Provides static methods for collision detection between entities and tiles in a tile map.
/// </summary>
/// <remarks>
/// This utility class handles spatial queries and collision resolution for tile-based collision detection.
/// It operates on tile layers marked as collidable and provides efficient bounding box intersection tests.
/// All methods are stateless and thread-safe for read-only tile map operations.
/// </remarks>
public static class TileCollider
{
    /// <summary>
    /// Checks if a rectangular bounds collides with any collidable tile in the specified layer.
    /// </summary>
    /// <param name="tileMap">The tile map to check collision against. Must not be <c>null</c>.</param>
    /// <param name="layerName">The name of the collision layer to query. Case-sensitive.</param>
    /// <param name="bounds">The entity bounds in world coordinates (pixels) to test for collision.</param>
    /// <returns>
    /// <c>true</c> if the bounds intersect with at least one collidable tile in the layer;
    /// otherwise, <c>false</c>. Returns <c>false</c> if the layer does not exist.
    /// </returns>
    /// <remarks>
    /// This method converts world coordinates to tile coordinates and performs an AABB
    /// (Axis-Aligned Bounding Box) test against all tiles within the bounds.
    /// Only tiles with <see cref="Tile.IsCollidable"/> set to <c>true</c> are considered.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    public static System.Boolean CheckCollision(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer == null)
        {
            return false;
        }

        // Convert bounds to tile coordinates
        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(new Vector2f(bounds.Left + bounds.Width, bounds.Top + bounds.Height));

        // Check all tiles in the bounds
        for (System.Int32 y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (System.Int32 x = topLeft.X; x <= bottomRight.X; x++)
            {
                if (!tileMap.IsValidTileCoord(x, y))
                {
                    continue;
                }

                Tile tile = layer.GetTile(x, y);
                if (tile?.IsCollidable == true)
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
    /// <param name="tileMap">The tile map to query. Must not be <c>null</c>.</param>
    /// <param name="layerName">The name of the collision layer to search. Case-sensitive.</param>
    /// <param name="bounds">The rectangular area in world coordinates (pixels) to check.</param>
    /// <returns>
    /// A list of <see cref="Tile"/> objects that are collidable and intersect with the bounds.
    /// Returns an empty list if no collisions are found or if the layer does not exist.
    /// The returned list is never <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for detailed collision response where individual tile data is needed,
    /// such as determining tile properties or performing per-tile collision resolution.
    /// The tiles are returned in row-major order (left-to-right, top-to-bottom).
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    public static System.Collections.Generic.List<Tile> GetCollidingTiles(TileMap tileMap, System.String layerName, FloatRect bounds)
    {
        System.Collections.Generic.List<Tile> result = [];

        TileLayer layer = tileMap.GetLayer(layerName);
        if (layer == null)
        {
            return result;
        }

        Vector2i topLeft = tileMap.WorldToTile(new Vector2f(bounds.Left, bounds.Top));
        Vector2i bottomRight = tileMap.WorldToTile(new Vector2f(bounds.Left + bounds.Width, bounds.Top + bounds.Height));

        for (System.Int32 y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (System.Int32 x = topLeft.X; x <= bottomRight.X; x++)
            {
                if (!tileMap.IsValidTileCoord(x, y))
                {
                    continue;
                }

                Tile tile = layer.GetTile(x, y);
                if (tile?.IsCollidable == true)
                {
                    result.Add(tile);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether moving an entity from its current position to a new position would cause a collision.
    /// </summary>
    /// <param name="tileMap">The tile map to check collision against. Must not be <c>null</c>.</param>
    /// <param name="layerName">The name of the collision layer to query. Case-sensitive.</param>
    /// <param name="currentPos">The current position of the entity in world coordinates. This parameter is not used in collision detection.</param>
    /// <param name="newPos">The target position to test in world coordinates (pixels).</param>
    /// <param name="size">The size of the entity's bounding box in pixels.</param>
    /// <returns>
    /// <c>true</c> if moving to <paramref name="newPos"/> would result in a collision with any collidable tile;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that constructs a bounding box at the new position and calls
    /// <see cref="CheckCollision"/>. The <paramref name="currentPos"/> parameter is not used for collision detection.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static System.Boolean WouldCollide(TileMap tileMap, System.String layerName, Vector2f currentPos, Vector2f newPos, Vector2f size)
    {
        FloatRect newBounds = new(newPos.X, newPos.Y, size.X, size.Y);
        return CheckCollision(tileMap, layerName, newBounds);
    }

    /// <summary>
    /// Resolves a collision by finding a valid position closest to the target position.
    /// </summary>
    /// <param name="tileMap">The tile map to perform collision resolution against. Must not be <c>null</c>.</param>
    /// <param name="layerName">The name of the collision layer to use. Case-sensitive.</param>
    /// <param name="currentPos">The current valid position of the entity in world coordinates (pixels).</param>
    /// <param name="targetPos">The desired target position in world coordinates (pixels).</param>
    /// <param name="size">The size of the entity's bounding box in pixels.</param>
    /// <returns>
    /// A resolved position in world coordinates. Returns <paramref name="targetPos"/> if no collision occurs,
    /// a position with movement on one axis if sliding is possible, or <paramref name="currentPos"/> if
    /// movement on both axes would cause collision.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements a simple sliding collision response by testing movement on each axis independently:
    /// </para>
    /// <list type="number">
    /// <item>First, it tests horizontal (X) movement while keeping vertical (Y) position unchanged.</item>
    /// <item>If X movement is valid, it returns the position with only Y at current position (sliding along Y).</item>
    /// <item>If X movement collides, it tests vertical (Y) movement while keeping horizontal (X) position unchanged.</item>
    /// <item>If Y movement is valid, it returns the position with only X at current position (sliding along X).</item>
    /// <item>If both axes collide independently, it returns the current position (no movement).</item>
    /// </list>
    /// <para>
    /// This provides a basic "slide along walls" behavior common in 2D tile-based games.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    public static Vector2f ResolveCollision(TileMap tileMap, System.String layerName, Vector2f currentPos, Vector2f targetPos, Vector2f size)
    {
        Vector2f resolved = targetPos;

        // Try X movement only
        FloatRect xBounds = new(targetPos.X, currentPos.Y, size.X, size.Y);
        if (!CheckCollision(tileMap, layerName, xBounds))
        {
            resolved.Y = currentPos.Y;
            return resolved;
        }

        // Try Y movement only
        FloatRect yBounds = new(currentPos.X, targetPos.Y, size.X, size.Y);
        if (!CheckCollision(tileMap, layerName, yBounds))
        {
            resolved.X = currentPos.X;
            return resolved;
        }

        // Both axes collide, stay at current position
        return currentPos;
    }
}