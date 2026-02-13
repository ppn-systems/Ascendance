// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using SFML.System;

namespace Ascendance.Movement;

/// <summary>
/// Implements running movement with increased speed for top-down 2D games.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RunMovement"/> class with custom speed.
/// </remarks>
/// <param name="speed">The running speed in pixels per second.</param>
public class RunMovement(System.Single speed) : IMovement
{
    private readonly System.Single _speed = speed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMovement"/> class with default speed.
    /// </summary>
    public RunMovement() : this(500f)
    {
    }

    /// <inheritdoc/>
    public void Move(
        ref Vector2f position,
        ref Vector2f velocity,
        Vector2f direction,
        System.Single deltaTime) => velocity = new Vector2f(direction.X * _speed, direction.Y * _speed);
}