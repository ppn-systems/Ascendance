// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Abstractions;
using Ascendance.Shared.Enums;
using SFML.System;

namespace Ascendance.Physics.Movement;

public class MovementController
{
    #region Fields

    private readonly System.Collections.Generic.Dictionary<MovementType, IMovement> _strategies;

    private Vector2f _direction;
    private MovementType _currentType;

    #endregion Fields

    #region Properties

    public Vector2f Position { get; set; }

    public Vector2f Velocity { get; private set; }

    #endregion Properties

    #region Constructors

    public MovementController(Vector2f initialPosition)
    {
        _currentType = MovementType.None;

        Position = initialPosition;
        Velocity = new Vector2f(0, 0);
        _strategies = new System.Collections.Generic.Dictionary<MovementType, IMovement>
        {
            { MovementType.Walk, new WalkMovement() },
        };
    }

    #endregion Constructors

    #region APIs

    public void SetMovement(MovementType movementType, Vector2f movementDirection)
    {
        _currentType = movementType;
        _direction = movementDirection;
    }

    public void Update(System.Single deltaTime)
    {
        // Use local variables for ref parameters, then assign back to properties
        Vector2f position = Position;
        Vector2f velocity = Velocity;

        if (_strategies.TryGetValue(_currentType, out IMovement strategy))
        {
            strategy.Move(ref position, ref velocity, _direction, deltaTime);
        }

        position += velocity * deltaTime;

        // Giả lập tiếp đất
        if (position.Y > 0)
        {
            position = new Vector2f(position.X, 0);
            velocity = new Vector2f(velocity.X, 0);
        }

        // Assign back to properties
        Position = position;
        Velocity = velocity;
    }

    #endregion APIs
}