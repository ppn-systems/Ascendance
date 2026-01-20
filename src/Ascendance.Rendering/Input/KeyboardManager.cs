// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Internal.Input;
using Nalix.Framework.Injection.DI;
using SFML.Window;

namespace Ascendance.Rendering.Input;

/// <summary>
/// Manages keyboard state and input.
/// </summary>
public class KeyboardManager : SingletonBase<KeyboardManager>
{
    #region Fields

    private System.Boolean _inputEnabled;
    private readonly Keyboard.Key[] AllKeys;
    private readonly System.Boolean[] KeyState;
    private readonly System.Boolean[] PreviousKeyState;

    #endregion Fields

    #region Properties

    public System.Boolean IsInputEnabled => _inputEnabled;

    #endregion Properties

    #region Constructor

    public KeyboardManager()
    {
        _inputEnabled = true;

        AllKeys = System.Enum.GetValues<Keyboard.Key>();
        KeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
        PreviousKeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
    }

    #endregion Constructor

    #region Input Control

    /// <summary>
    /// Enables all input globally.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EnableInput() => _inputEnabled = true;

    /// <summary>
    /// Disables all input globally.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void DisableInput() => _inputEnabled = false;

    public void Update()
    {
        if (!_inputEnabled)
        {
            System.Array.Fill(KeyState, false);
            return;
        }

        for (System.Int32 i = 0; i < AllKeys.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllKeys[i];

            if (idx < 0 || idx >= KeyState.Length)
            {
                continue;
            }

            PreviousKeyState[idx] = KeyState[idx];
            KeyState[idx] = Keyboard.IsKeyPressed(AllKeys[i]);
        }

        if (InputTimeline.Instance.IsRecording)
        {
            KeyState.Clone();
        }
    }

    #endregion Input Control

    #region Getters

    /// <summary>
    /// Gets all currently pressed keys.
    /// </summary>
    /// <returns>An enumerable containing the pressed keyboard keys.</returns>
    public System.Collections.Generic.IEnumerable<Keyboard.Key> GetPressedKeys()
    {
        for (System.Int32 i = 0; i < KeyState.Length; i++)
        {
            if (KeyState[i])
            {
                yield return (Keyboard.Key)i;
            }
        }
    }

    #endregion Getters

    #region Keyboard

    /// <summary>
    /// Checks if a key is currently being pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyDown(Keyboard.Key key) => KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key is currently not being pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently up; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyUp(Keyboard.Key key) => !KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was pressed for the first time this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was pressed this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyPressed(Keyboard.Key key) => KeyState[(System.Int32)key] && !PreviousKeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was released for the first time this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was released this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyReleased(Keyboard.Key key) => !KeyState[(System.Int32)key] && PreviousKeyState[(System.Int32)key];

    #endregion Keyboard

    #region Internal Methods

    public System.Boolean[] CreateKeyboardStateSnapshot()
    {
        System.Boolean[] arr = new System.Boolean[KeyState.Length];
        KeyState.CopyTo(arr, 0);

        return arr;
    }

    public void RestoreKeyboardState(System.Boolean[] snapshot)
    {
        if (snapshot.Length != KeyState.Length)
        {
            throw new System.ArgumentException("Invalid key state length");
        }

        for (System.Int32 i = 0; i < KeyState.Length; i++)
        {
            KeyState[i] = snapshot[i];
        }
    }

    #endregion Internal Methods
}