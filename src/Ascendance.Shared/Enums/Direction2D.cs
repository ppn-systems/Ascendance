// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Shared.Enums;

/// <summary>
/// Represents 2D directional facing for sprites and entities.
/// </summary>
public enum Direction2D : System.Byte
{
    /// <summary>
    /// Facing upward (North).
    /// </summary>
    Up = 0,

    /// <summary>
    /// Facing downward (South).
    /// </summary>
    Down = 1,

    /// <summary>
    /// Facing left (West).
    /// </summary>
    Left = 2,

    /// <summary>
    /// Facing right (East).
    /// </summary>
    Right = 3,

    /// <summary>
    /// Facing up-left (Northwest).
    /// </summary>
    UpLeft = 4,

    /// <summary>
    /// Facing up-right (Northeast).
    /// </summary>
    UpRight = 5,

    /// <summary>
    /// Facing down-left (Southwest).
    /// </summary>
    DownLeft = 6,

    /// <summary>
    /// Facing down-right (Southeast).
    /// </summary>
    DownRight = 7
}