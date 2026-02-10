// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Abstractions;
using SFML.System;

namespace Ascendance.Physics.Movement;

public class RunMovement : IMovement
{
    private readonly System.Single _speed = 200f;

    public void Move(ref Vector2f position, ref Vector2f velocity, Vector2f direction, System.Single deltaTime) => velocity.X = direction.X * _speed;
}
