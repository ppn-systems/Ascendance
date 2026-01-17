// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Physics.Colliders;

namespace Ascendance.Physics.Physics;

/// <summary>
/// Static utility for collision detection between any collider types.
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// Determines if two colliders are colliding.
    /// </summary>
    /// <param name="a">The first collider.</param>
    /// <param name="b">The second collider.</param>
    /// <returns>True if the colliders overlap; otherwise, false.</returns>
    public static System.Boolean IsColliding(ICollider a, ICollider b) => a.IsColliding(b);
}