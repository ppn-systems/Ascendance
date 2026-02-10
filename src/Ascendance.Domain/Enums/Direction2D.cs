// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Domain.Enums;

/// <summary>
/// Defines the four cardinal directions a player can face in 2.5D top-down perspective.
/// </summary>
/// <remarks>
/// Used for animation frame selection and sprite rendering.
/// Order matches standard tileset convention (Down, Up, Left, Right).
/// </remarks>
public enum Direction2D : System.Byte
{
    /// <summary>
    /// Player is facing downward (toward the camera/screen bottom).
    /// </summary>
    /// <remarks>
    /// Typically corresponds to row 0 in sprite sheets.
    /// </remarks>
    Down = 0,

    /// <summary>
    /// Player is facing upward (away from camera/screen top).
    /// </summary>
    /// <remarks>
    /// Typically corresponds to row 1 in sprite sheets.
    /// </remarks>
    Up = 1,

    /// <summary>
    /// Player is facing left.
    /// </summary>
    /// <remarks>
    /// Typically corresponds to row 2 in sprite sheets.
    /// </remarks>
    Left = 2,

    /// <summary>
    /// Player is facing right.
    /// </summary>
    /// <remarks>
    /// Typically corresponds to row 3 in sprite sheets.
    /// </remarks>
    Right = 3
}