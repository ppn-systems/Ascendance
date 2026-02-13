// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Abstractions;

/// <summary>
/// Defines a movement strategy for an entity.
/// Implementations encapsulate specific movement behaviors
/// such as walking, jumping, flying, or swimming.
/// </summary>
public interface IMovement
{
    /// <summary>
    /// Updates the position and velocity of an entity based on movement rules.
    /// </summary>
    /// <param name="position">
    /// The current world position of the entity.
    /// Passed by reference to allow direct modification.
    /// </param>
    /// <param name="velocity">
    /// The current velocity of the entity.
    /// Passed by reference to apply acceleration, gravity, or damping.
    /// </param>
    /// <param name="direction">
    /// The normalized movement direction input (e.g. from player or AI).
    /// </param>
    /// <param name="deltaTime">
    /// The elapsed time since the last update, in seconds.
    /// Used to ensure frame-rate independent movement.
    /// </param>
    void Move(ref Vector2f position, ref Vector2f velocity, Vector2f direction, System.Single deltaTime);
}
