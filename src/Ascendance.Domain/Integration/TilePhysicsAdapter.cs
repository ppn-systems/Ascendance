// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Physics.Colliders;
using Ascendance.Physics.Physics;
using Ascendance.Rendering.Tiles;
using SFML.System;

namespace Ascendance.Domain.Integration;

/// <summary>
/// Provides integration between tile-based collision detection and the physics engine.
/// </summary>
/// <remarks>
/// <para>
/// This adapter class bridges the gap between static tile-based collision systems
/// (from <see cref="Rendering.Tiles"/>) and dynamic physics simulations
/// (from <see cref="Physics"/>). It enables <see cref="RigidBody"/> objects
/// to interact with collidable tiles in a <see cref="TileMap"/>.
/// </para>
/// <para>
/// Supported collider types:
/// </para>
/// <list type="bullet">
/// <item><see cref="RectangleCollider"/>: Axis-aligned bounding box collision with sliding resolution</item>
/// <item><see cref="CircleCollider"/>: Bounding box approximation for tile collision detection</item>
/// </list>
/// <para>
/// This class is thread-safe for read-only operations on the tile map, but physics resolution
/// should occur on the main simulation thread to avoid race conditions with velocity updates.
/// </para>
/// </remarks>
public sealed class TilePhysicsAdapter
{
    #region Fields

    /// <summary>
    /// The tile map containing collidable tiles.
    /// </summary>
    private readonly TileMap _tileMap;

    /// <summary>
    /// The name of the collision layer to query in the tile map.
    /// </summary>
    private readonly System.String _collisionLayer;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TilePhysicsAdapter"/> class.
    /// </summary>
    /// <param name="tileMap">
    /// The <see cref="TileMap"/> containing collidable tiles. Must not be <c>null</c>.
    /// </param>
    /// <param name="collisionLayer">
    /// The name of the layer in the tile map that contains collision tiles.
    /// Typically named "Collision" or "Walls" in the Tiled map editor.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// The collision layer should have tiles with <see cref="Tile.IsCollidable"/> set to <c>true</c>
    /// for proper collision detection. If the specified layer does not exist in the tile map,
    /// collision checks will always return <c>false</c>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public TilePhysicsAdapter(TileMap tileMap, System.String collisionLayer)
    {
        _tileMap = tileMap ?? throw new System.ArgumentNullException(nameof(tileMap));
        _collisionLayer = collisionLayer;
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Determines whether a rigid body collides with any collidable tile in the collision layer.
    /// </summary>
    /// <param name="body">
    /// The <see cref="RigidBody"/> to test for collision. Must have a valid <see cref="RigidBody.Collider"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the rigid body's collider intersects with at least one collidable tile;
    /// otherwise, <c>false</c>. Returns <c>false</c> if the collider type is not supported.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs collision detection based on the rigid body's collider type:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="RectangleCollider"/></term>
    /// <description>Uses the rectangle's bounds directly for AABB collision testing.</description>
    /// </item>
    /// <item>
    /// <term><see cref="CircleCollider"/></term>
    /// <description>
    /// Uses an axis-aligned bounding box approximation around the circle.
    /// The bounding box is calculated as: position - radius to position + radius.
    /// This may produce false positives at the circle's corners but ensures all actual collisions are detected.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// For unsupported collider types, this method returns <c>false</c>.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="body"/> is <c>null</c>.
    /// </exception>
    public System.Boolean CheckCollisionWithTiles(RigidBody body)
    {
        if (body.Collider is RectangleCollider rect)
        {
            SFML.Graphics.FloatRect bounds = new(
                rect.Position.X,
                rect.Position.Y,
                rect.Size.X,
                rect.Size.Y
            );

            return TileCollider.CheckCollision(_tileMap, _collisionLayer, bounds);
        }

        if (body.Collider is CircleCollider circle)
        {
            // Create bounding box for circle
            System.Single diameter = circle.Radius * 2;
            SFML.Graphics.FloatRect bounds = new(
                circle.Position.X - circle.Radius,
                circle.Position.Y - circle.Radius,
                diameter,
                diameter
            );

            return TileCollider.CheckCollision(_tileMap, _collisionLayer, bounds);
        }

        return false;
    }

    /// <summary>
    /// Resolves collision between a rigid body and tiles, applying physics response to velocity and position.
    /// </summary>
    /// <param name="body">
    /// The <see cref="RigidBody"/> to resolve collision for. Must have a <see cref="RectangleCollider"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method performs collision resolution in the following steps:
    /// </para>
    /// <list type="number">
    /// <item>
    /// Calculates the target position based on current velocity assuming 60 FPS (0.016s frame time).
    /// </item>
    /// <item>
    /// Attempts to resolve collision using <see cref="TileCollider.ResolveCollision"/>,
    /// which implements sliding collision response.
    /// </item>
    /// <item>
    /// Detects which axis (X, Y, or both) was blocked by comparing resolved position to target position.
    /// </item>
    /// <item>
    /// Zeroes out velocity on blocked axes to simulate inelastic collision (no bounce).
    /// </item>
    /// <item>
    /// Updates the collider's position to the resolved position.
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Limitations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Only <see cref="RectangleCollider"/> is supported. Other collider types are ignored.</item>
    /// <item>Frame time is hardcoded to 0.016s (60 FPS). Variable timesteps may cause inaccuracies.</item>
    /// <item>Collision response is fully inelastic (no bounce). Elasticity/restitution is not considered.</item>
    /// <item>Does not apply friction or other surface properties from tiles.</item>
    /// </list>
    /// <para>
    /// <strong>Usage:</strong> This method should be called during the physics update loop after velocity
    /// has been integrated but before final position updates. It modifies both the rigid body's velocity
    /// and its collider's position.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="body"/> is <c>null</c>.
    /// </exception>
    public void ResolveCollisionWithTiles(RigidBody body)
    {
        if (body.Collider is not RectangleCollider rect)
        {
            return;
        }

        Vector2f currentPos = rect.Position;
        Vector2f size = rect.Size;

        // Try to resolve using TileCollider's resolution
        Vector2f targetPos = new(
            currentPos.X + (body.Velocity.X * 0.016f), // Assume 60fps
            currentPos.Y + (body.Velocity.Y * 0.016f)
        );

        Vector2f resolved = TileCollider.ResolveCollision(
            _tileMap,
            _collisionLayer,
            currentPos,
            targetPos,
            size
        );

        // Apply physics response
        if (resolved.X == currentPos.X && targetPos.X != currentPos.X)
        {
            // Horizontal collision - stop horizontal velocity
            body.Velocity = new Vector2f(0, body.Velocity.Y);
        }

        if (resolved.Y == currentPos.Y && targetPos.Y != currentPos.Y)
        {
            // Vertical collision - stop vertical velocity
            body.Velocity = new Vector2f(body.Velocity.X, 0);
        }

        // Update position
        rect.Position = resolved;
    }

    #endregion Methods
}