// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Contracts.Enums;

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
    /// Rapid movement, typically faster than walking.
    /// </summary>
    Run = 2
}
