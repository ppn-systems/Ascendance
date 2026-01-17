// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Physics.Colliders;

/// <summary>
/// Represents a circle collider.
/// </summary>
public class CircleCollider : ICollider
{
    /// <inheritdoc/>
    public Vector2f Position { get; set; }
    /// <summary>
    /// Gets or sets the radius of the circle.
    /// </summary>
    public System.Single Radius { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircleCollider"/> class.
    /// </summary>
    /// <param name="position">The center position.</param>
    /// <param name="radius">The radius.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public CircleCollider(Vector2f position, System.Single radius)
    {
        this.Radius = radius;
        this.Position = position;
    }

    /// <inheritdoc/>
    public System.Boolean IsColliding(ICollider other)
    {
        if (other is CircleCollider circle)
        {
            System.Single dx = this.Position.X - circle.Position.X;
            System.Single dy = this.Position.Y - circle.Position.Y;
            System.Single distanceSq = (dx * dx) + (dy * dy);
            System.Single radiusSum = this.Radius + circle.Radius;
            return distanceSq < (radiusSum * radiusSum);
        }

        return other is RectangleCollider rect && rect.IsColliding(this);
    }
}