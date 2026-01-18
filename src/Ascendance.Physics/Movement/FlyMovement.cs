// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Abstractions;
using SFML.System;

namespace Ascendance.Physics.Movement;

public class FlyMovement : IMovement
{
    private readonly System.Single _speed = 180f;

    public void Move(
        ref Vector2f position, ref Vector2f velocity, Vector2f direction,
        ref System.Boolean isGrounded, System.Single deltaTime)
    {
        if (isGrounded)
        {
            velocity.X = direction.X * _speed;
            velocity.Y = direction.Y * _speed;
            isGrounded = false;
        }
    }
}
