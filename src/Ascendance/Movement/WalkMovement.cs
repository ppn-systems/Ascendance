// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using SFML.System;

namespace Ascendance.Movement;

/// <summary>
/// Implements walking movement for top-down 2D games.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WalkMovement"/> class with custom speed.
/// </remarks>
/// <param name="speed">The walking speed in pixels per second.</param>
public class WalkMovement(System.Single speed) : IMovement
{
    private readonly System.Single _speed = speed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalkMovement"/> class with default speed.
    /// </summary>
    public WalkMovement() : this(200f)
    {
    }

    /// <inheritdoc/>
    public void Move(
        ref Vector2f position,
        ref Vector2f velocity,
        Vector2f direction,
        System.Single deltaTime) => velocity = new Vector2f(direction.X * _speed, direction.Y * _speed);
}