// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Domain.Enums;

/// <summary>
/// Specifies the collision resolution strategy for tile-based collision detection.
/// </summary>
/// <remarks>
/// Defines how the collision system should respond when an entity collides with tiles.
/// </remarks>
public enum CollisionResolutionMode
{
    /// <summary>
    /// No collision resolution (detection only). Returns collision state without modifying position.
    /// </summary>
    None = 0,

    /// <summary>
    /// Slide along walls by testing movement on each axis independently.
    /// </summary>
    /// <remarks>
    /// This mode allows entities to slide along walls when moving diagonally into a corner.
    /// X and Y axes are resolved separately in order of priority.
    /// </remarks>
    Slide = 1,

    /// <summary>
    /// Stop all movement on collision (snap back to previous position).
    /// </summary>
    /// <remarks>
    /// The most restrictive collision response. Entity cannot move if any part of the
    /// movement would result in collision.
    /// </remarks>
    Stop = 2,

    /// <summary>
    /// Push out to the nearest non-colliding position.
    /// </summary>
    /// <remarks>
    /// Finds the closest valid position when collision is detected.
    /// Useful for preventing entities from getting stuck in walls.
    /// </remarks>
    Push = 3
}
