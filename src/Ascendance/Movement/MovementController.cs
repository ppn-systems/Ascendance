// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Abstractions;
using Ascendance.Shared.Enums;
using SFML.System;

namespace Ascendance.Movement;

/// <summary>
/// Controls movement state and applies movement strategies to update position and velocity.
/// </summary>
public class MovementController
{
    #region Fields

    private readonly System.Collections.Generic.Dictionary<MovementType, IMovement> _strategies;

    private Vector2f _direction;
    private MovementType _currentType;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the current world position of the entity.
    /// </summary>
    /// <remarks>
    /// Position is expressed in world units used by the rendering/physics system.
    /// </remarks>
    public Vector2f Position { get; set; }

    /// <summary>
    /// Gets the current linear velocity of the entity.
    /// </summary>
    /// <remarks>
    /// Velocity is expressed in units per second.
    /// </remarks>
    public Vector2f Velocity { get; private set; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MovementController"/> class.
    /// </summary>
    /// <param name="initialPosition">The initial world position of the entity.</param>
    public MovementController(Vector2f initialPosition)
    {
        _currentType = MovementType.None;

        Position = initialPosition;
        Velocity = new Vector2f(0, 0);
        _strategies = new System.Collections.Generic.Dictionary<MovementType, IMovement>
        {
            { MovementType.Run, new RunMovement() },
            { MovementType.Walk, new WalkMovement() }
        };
    }

    #endregion Constructors

    #region APIs

    /// <summary>
    /// Sets the current movement type and direction.
    /// </summary>
    /// <param name="movementType">The movement strategy to use (for example, Walk or Run).</param>
    /// <param name="movementDirection">Normalized direction vector indicating movement heading. Zero vector indicates no input.</param>
    public void SetMovement(MovementType movementType, Vector2f movementDirection)
    {
        _currentType = movementType;
        _direction = movementDirection;
    }

    /// <summary>
    /// Updates the controller, applying the active movement strategy and integrating velocity into position.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds since the last update. Must be non-negative.</param>
    public void Update(System.Single deltaTime)
    {
        // Use local variables for ref parameters, then assign back to properties
        Vector2f position = Position;
        Vector2f velocity = Velocity;

        // Apply movement strategy if one is active
        if (_strategies.TryGetValue(_currentType, out IMovement strategy))
        {
            strategy.Move(ref position, ref velocity, _direction, deltaTime);
        }
        else
        {
            // No movement type set - stop movement
            velocity = new Vector2f(0, 0);
        }

        // Apply velocity to position (top-down 2D - no gravity)
        position += velocity * deltaTime;

        // Assign back to properties
        Position = position;
        Velocity = velocity;
    }

    #endregion APIs
}