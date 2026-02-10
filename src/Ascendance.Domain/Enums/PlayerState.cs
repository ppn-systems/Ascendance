// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Domain.Enums;

/// <summary>
/// Defines the possible states a player character can be in.
/// </summary>
/// <remarks>
/// Each state typically has associated animations and gameplay behavior.
/// States are mutually exclusive (player can only be in one state at a time).
/// </remarks>
public enum PlayerState : System.Byte
{
    /// <summary>
    /// Player is standing still with no movement input.
    /// </summary>
    /// <remarks>
    /// Plays idle animation (usually frame 0 of walk cycle or dedicated idle frames).
    /// </remarks>
    Idle = 0,

    /// <summary>
    /// Player is walking at normal speed.
    /// </summary>
    /// <remarks>
    /// Plays walk cycle animation (4 frames per direction).
    /// Uses <see cref="Physics.Movement.WalkMovement"/> strategy.
    /// </remarks>
    Walking = 1,

    /// <summary>
    /// Player is running at increased speed (typically while holding Shift).
    /// </summary>
    /// <remarks>
    /// Plays walk cycle animation at faster frame rate.
    /// Uses <see cref="RunMovement"/> strategy with higher speed multiplier.
    /// </remarks>
    Running = 2,

    /// <summary>
    /// Player is using a tool or performing an action.
    /// </summary>
    /// <remarks>
    /// Reserved for future implementation (axe swing, pickaxe, watering can, etc.).
    /// Movement may be restricted during this state.
    /// </remarks>
    UsingTool = 3,

    /// <summary>
    /// Player is interacting with an object or NPC.
    /// </summary>
    /// <remarks>
    /// Reserved for future implementation (talking to NPC, opening chest, etc.).
    /// Movement is typically disabled during this state.
    /// </remarks>
    Interacting = 4
}