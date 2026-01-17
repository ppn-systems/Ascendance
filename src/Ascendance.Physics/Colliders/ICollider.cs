// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Physics.Colliders;

/// <summary>
/// Represents a generic collider used for collision detection.
/// </summary>
public interface ICollider
{
    /// <summary>
    /// Determines whether this collider is colliding with another collider.
    /// </summary>
    /// <param name="other">The other collider to test against.</param>
    /// <returns>
    /// <see langword="true"/> if the colliders are intersecting; otherwise, <see langword="false"/>.
    /// </returns>
    System.Boolean IsColliding(ICollider other);

    /// <summary>
    /// Gets or sets the center position of the collider in world space.
    /// </summary>
    Vector2f Position { get; set; }
}
