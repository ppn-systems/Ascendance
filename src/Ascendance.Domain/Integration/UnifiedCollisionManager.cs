// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Physics.Physics;
using Ascendance.Rendering.Tiles;

namespace Ascendance.Domain.Integration;

/// <summary>
/// Provides unified collision management for both dynamic-to-dynamic and dynamic-to-static interactions.
/// </summary>
/// <remarks>
/// <para>
/// This manager orchestrates collision detection and resolution across two separate systems:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>Dynamic-to-Dynamic</term>
/// <description>
/// Collisions between moving <see cref="RigidBody"/> objects, handled by <see cref="CollisionManager"/>.
/// </description>
/// </item>
/// <item>
/// <term>Dynamic-to-Static</term>
/// <description>
/// Collisions between moving <see cref="RigidBody"/> objects and static <see cref="TileMap"/> geometry,
/// handled by <see cref="TilePhysicsAdapter"/>.
/// </description>
/// </item>
/// </list>
/// <para>
/// The collision processing order is important: dynamic collisions are resolved first, followed by
/// tile collisions. This ensures that inter-body physics is computed before applying environmental constraints.
/// </para>
/// <para>
/// This class is designed to be updated once per physics timestep, typically during the fixed update loop.
/// </para>
/// </remarks>
public sealed class UnifiedCollisionManager
{
    #region Fields

    /// <summary>
    /// Manages collisions between dynamic rigid bodies.
    /// </summary>
    private readonly CollisionManager _dynamicCollisions;

    /// <summary>
    /// Manages collisions between dynamic rigid bodies and static tile geometry.
    /// </summary>
    private readonly TilePhysicsAdapter _tileAdapter;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedCollisionManager"/> class.
    /// </summary>
    /// <param name="bodies">
    /// The collection of <see cref="RigidBody"/> objects to manage for dynamic collisions.
    /// Must not be <c>null</c>. This collection should include all dynamic entities in the physics simulation.
    /// </param>
    /// <param name="tileMap">
    /// The <see cref="TileMap"/> containing static collision geometry. Must not be <c>null</c>.
    /// </param>
    /// <param name="collisionLayer">
    /// The name of the layer in the <paramref name="tileMap"/> that contains collidable tiles.
    /// Typically named "Collision", "Walls", or "Solid" in the Tiled map editor.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="bodies"/> or <paramref name="tileMap"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="bodies"/> collection is shared between this manager and the underlying
    /// <see cref="CollisionManager"/>. Changes to the collection (adding/removing bodies) will
    /// automatically be reflected in collision detection.
    /// </para>
    /// <para>
    /// The <paramref name="collisionLayer"/> should reference a layer where tiles have their
    /// <see cref="Tile.IsCollidable"/> property set to <c>true</c>. If the layer does not exist
    /// or contains no collidable tiles, dynamic-to-static collision detection will not occur.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public UnifiedCollisionManager(
        System.Collections.Generic.List<RigidBody> bodies,
        TileMap tileMap,
        System.String collisionLayer)
    {
        _dynamicCollisions = new CollisionManager(bodies);
        _tileAdapter = new TilePhysicsAdapter(tileMap, collisionLayer);
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Updates collision detection and resolution for all dynamic-to-dynamic and dynamic-to-static interactions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method processes collisions in the following order:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <description>
    /// <strong>Dynamic-to-Dynamic Collisions:</strong> All rigid body pairs are tested for collision
    /// via <see cref="CollisionManager.HandleCollisions"/>. This resolves inter-body physics such as
    /// momentum transfer, impulse response, and separation.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Dynamic-to-Static Collisions:</strong> Each rigid body is tested against the tile map
    /// collision layer. If a collision is detected, the body's position and velocity are adjusted
    /// to resolve penetration and apply collision response (typically stopping movement on the collision axis).
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Processing Order Rationale:</strong>
    /// </para>
    /// <para>
    /// Dynamic collisions are handled first to ensure that forces and impulses between moving bodies
    /// are resolved before environmental constraints (tile collisions) are applied. This prevents
    /// situations where tile collision resolution could interfere with inter-body physics calculations.
    /// </para>
    /// <para>
    /// <strong>Performance Considerations:</strong>
    /// </para>
    /// <para>
    /// Dynamic collision detection has O(n²) complexity for n bodies (though spatial partitioning may improve this).
    /// Tile collision checking is O(n × t) where t is the average number of tiles tested per body (typically small
    /// due to spatial queries). For large numbers of bodies or complex tile maps, consider optimizations such as:
    /// </para>
    /// <list type="bullet">
    /// <item>Broad-phase collision culling (spatial hashing, quadtrees)</item>
    /// <item>Sleeping/inactive body detection</item>
    /// <item>Tile collision caching</item>
    /// </list>
    /// <para>
    /// <strong>Usage:</strong> This method should be called once per physics timestep, typically in a fixed update loop:
    /// </para>
    /// <code>
    /// void FixedUpdate(float deltaTime)
    /// {
    ///     // Integrate velocities
    ///     foreach (var body in bodies)
    ///         body.Update(deltaTime);
    ///
    ///     // Resolve collisions
    ///     collisionManager.Update();
    /// }
    /// </code>
    /// </remarks>
    public void Update()
    {
        // Handle dynamic body collisions
        _dynamicCollisions.HandleCollisions();

        // Handle tile collisions for each body
        foreach (RigidBody body in _dynamicCollisions.Bodies)
        {
            if (_tileAdapter.CheckCollisionWithTiles(body))
            {
                _tileAdapter.ResolveCollisionWithTiles(body);
            }
        }
    }

    #endregion Methods
}