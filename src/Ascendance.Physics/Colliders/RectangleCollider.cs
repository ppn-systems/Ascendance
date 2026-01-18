// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Physics.Colliders;

/// <summary>
/// Represents a rectangle collider.
/// </summary>
public class RectangleCollider : ICollider
{
    #region Properties

    /// <inheritdoc/>
    public Vector2f Position { get; set; }

    /// <summary>
    /// Gets or sets the size of the rectangle.
    /// </summary>
    public Vector2f Size { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleCollider"/> class.
    /// </summary>
    /// <param name="position">The top-left position.</param>
    /// <param name="size">The size of the rectangle.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public RectangleCollider(Vector2f position, Vector2f size)
    {
        this.Size = size;
        this.Position = position;
    }

    #endregion Constructor

    #region Methods

    /// <inheritdoc/>
    public System.Boolean IsColliding(ICollider other)
    {
        if (other is RectangleCollider rect)
        {
            return this.Position.X < rect.Position.X + rect.Size.X &&
                   this.Position.X + this.Size.X > rect.Position.X &&
                   this.Position.Y < rect.Position.Y + rect.Size.Y &&
                   this.Position.Y + this.Size.Y > rect.Position.Y;
        }

        if (other is CircleCollider circle)
        {
            System.Single closestX = System.MathF.Max(Position.X, System.MathF.Min(circle.Position.X, Position.X + Size.X));
            System.Single closestY = System.MathF.Max(Position.Y, System.MathF.Min(circle.Position.Y, Position.Y + Size.Y));
            System.Single dx = circle.Position.X - closestX;
            System.Single dy = circle.Position.Y - closestY;
            return ((dx * dx) + (dy * dy)) < (circle.Radius * circle.Radius);
        }
        return false;
    }

    #endregion Methods
}