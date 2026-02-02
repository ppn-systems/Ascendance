// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using Nalix.Logging.Extensions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Input;

/// <summary>
/// Manages mouse state and input.
/// </summary>
[System.Diagnostics.DebuggerDisplay(
    "MouseManager | Pos=({_mousePosition.X},{_mousePosition.Y}), ButtonsDown={GetPressedButtonCount()}")]
public class MouseManager : SingletonBase<MouseManager>
{
    #region Fields

    private Vector2i _mousePosition;
    private readonly Mouse.Button[] AllButtons;
    private readonly System.Boolean[] MouseButtonState;
    private readonly System.Boolean[] PreviousMouseButtonState;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseManager"/> class,
    /// configuring button state and default input enabled state.
    /// </summary>
    public MouseManager()
    {
        AllButtons = System.Enum.GetValues<Mouse.Button>();
        MouseButtonState = new System.Boolean[(System.Int32)Mouse.Button.ButtonCount];
        PreviousMouseButtonState = new System.Boolean[(System.Int32)Mouse.Button.ButtonCount];
    }

    #endregion Constructor

    #region Input Control

    /// <summary>
    /// Updates mouse button state and position based on the current frame.
    /// </summary>
    /// <param name="window">The render window reference (for relative mouse position).</param>
    public void Update(RenderWindow window)
    {
        for (System.Int32 i = 0; i < AllButtons.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllButtons[i];

            if (AllButtons[i] == Mouse.Button.ButtonCount)
            {
                continue;
            }

            PreviousMouseButtonState[idx] = MouseButtonState[idx];
            MouseButtonState[idx] = Mouse.IsButtonPressed(AllButtons[i]);
        }

        _mousePosition = Mouse.GetPosition(window);
    }

    #endregion Input Control

    #region Getter Methods

    /// <summary>
    /// Gets the current position of the mouse, in window coordinates.
    /// </summary>
    /// <returns>A <see cref="Vector2i"/> containing the X and Y position of the mouse.</returns>
    public Vector2i GetMousePosition() => _mousePosition;

    /// <summary>
    /// Gets the current position of the mouse as a floating point tuple.
    /// </summary>
    /// <returns>A tuple containing the X and Y position of the mouse as floats.</returns>
    public (System.Single X, System.Single Y) GetMousePositionF() => (_mousePosition.X, _mousePosition.Y);

    #endregion Getter Methods

    #region Mouse Button State Methods

    /// <summary>
    /// Determines whether the specified mouse button is currently being held down.
    /// </summary>
    /// <param name="button">The mouse button to check (e.g., Left, Right).</param>
    /// <returns>True if the mouse button is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonDown(Mouse.Button button) => MouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just pressed during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button is down now but was not down in the previous frame; otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonPressed(Mouse.Button button) => MouseButtonState[(System.Int32)button] && !PreviousMouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just released during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button was down in the previous frame but is not down now; otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonReleased(Mouse.Button button) => !MouseButtonState[(System.Int32)button] && PreviousMouseButtonState[(System.Int32)button];

    #endregion Mouse Button State Methods

    #region Internal Methods

    /// <summary>
    /// Creates a snapshot array of the current mouse button state.
    /// </summary>
    /// <returns>
    /// A boolean array representing the current button state.
    /// </returns>
    internal System.Boolean[] CreateMouseButtonSnapshot()
    {
        System.Boolean[] arr = new System.Boolean[MouseButtonState.Length];
        MouseButtonState.CopyTo(arr, 0);
        return arr;
    }

    /// <summary>
    /// Restores the mouse button and position state from a snapshot.
    /// </summary>
    /// <param name="btnState">Boolean array representing mouse button states.</param>
    /// <param name="mousePos">The position to restore for the mouse.</param>
    /// <exception cref="System.ArgumentException">Thrown if the button state array length is invalid.</exception>
    internal void RestoreMouseState(System.Boolean[] btnState, Vector2i mousePos)
    {
        if (btnState.Length != MouseButtonState.Length)
        {
            NLogixFx.Error(
                message: $"RestoreMouseState: Invalid button state length {btnState.Length}, expected {MouseButtonState.Length}.",
                source: "MouseManager"
            );
            throw new System.ArgumentException("Invalid mouse button state length");
        }

        for (System.Int32 i = 0; i < MouseButtonState.Length; i++)
        {
            MouseButtonState[i] = btnState[i];
        }

        _mousePosition = mousePos;
        NLogixFx.Debug(message: $"Mouse state restored. Position=({mousePos.X},{mousePos.Y})", source: "MouseManager");
    }

    #endregion Internal Methods
}