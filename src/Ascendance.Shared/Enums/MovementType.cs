// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Shared.Enums;

/// <summary>
/// Represents the type of movement an entity can perform.
/// </summary>
public enum MovementType : System.Byte
{
    /// <summary>
    /// No movement. The entity remains idle.
    /// </summary>
    None = 0,

    /// <summary>
    /// Ground-based horizontal movement.
    /// </summary>
    Walk = 1,

    /// <summary>
    /// Vertical impulse movement initiated from the ground.
    /// </summary>
    Jump = 2,

    /// <summary>
    /// Free-form movement without gravity constraints.
    /// </summary>
    Fly = 3
}
