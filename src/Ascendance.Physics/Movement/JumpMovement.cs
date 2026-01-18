// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Abstractions;
using SFML.System;

namespace Ascendance.Physics.Movement;

public class JumpMovement : IMovement
{
    private readonly System.Single _speed = 300f;

    public void Move(
        ref Vector2f position, ref Vector2f velocity, Vector2f direction,
        ref System.Boolean isGrounded, System.Single deltaTime)
    {
        if (isGrounded)
        {
            velocity.Y = -_speed;
            isGrounded = false;
        }
    }
}
