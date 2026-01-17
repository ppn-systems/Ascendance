// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Shared.Abstractions;

/// <summary>
/// Strategy interface for movement behaviors.
/// </summary>
public interface IMovementBehavior
{
    void Move(ref Vector2f position, ref Vector2f velocity, Vector2f direction, ref System.Boolean isGrounded, System.Single deltaTime);
}
