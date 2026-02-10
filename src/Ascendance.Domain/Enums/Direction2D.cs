// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Domain.Enums;

/// <summary>
/// Defines the four cardinal directions a player can face in 2.5D top-down perspective.
/// </summary>
/// <remarks>
/// <para>
/// Used for animation frame selection and sprite rendering.
/// Order matches your sprite sheet layout:
/// </para>
/// <code>
/// Row 0: Down
/// Row 1: Right
/// Row 2: Up
/// Row 3: Left
/// </code>
/// </remarks>
public enum Direction2D : System.Byte
{
    /// <summary>
    /// Player is facing downward (toward the camera/screen bottom).
    /// </summary>
    /// <remarks>
    /// Corresponds to row 0 in the sprite sheet.
    /// </remarks>
    Down = 0,

    /// <summary>
    /// Player is facing right.
    /// </summary>
    /// <remarks>
    /// Corresponds to row 1 in the sprite sheet.
    /// </remarks>
    Right = 1,

    /// <summary>
    /// Player is facing upward (away from camera/screen top).
    /// </summary>
    /// <remarks>
    /// Corresponds to row 2 in the sprite sheet.
    /// </remarks>
    Up = 2,

    /// <summary>
    /// Player is facing left.
    /// </summary>
    /// <remarks>
    /// Corresponds to row 3 in the sprite sheet.
    /// </remarks>
    Left = 3
}