// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Internal.Input;
using Nalix.Framework.Injection.DI;
using Nalix.Logging.Extensions;
using SFML.Window;

namespace Ascendance.Rendering.Input;

/// <summary>
/// Manages keyboard state and input.
/// </summary>
public class KeyboardManager : SingletonBase<KeyboardManager>
{
    #region Fields

    private readonly Keyboard.Key[] AllKeys;
    private readonly System.Boolean[] KeyState;
    private readonly System.Boolean[] PreviousKeyState;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardManager"/> class,
    /// configuring all internal key state arrays.
    /// </summary>
    public KeyboardManager()
    {
        AllKeys = System.Enum.GetValues<Keyboard.Key>();
        KeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
        PreviousKeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
    }

    #endregion Constructor

    #region Input Control

    /// <summary>
    /// Updates the internal keyboard state for all keys.
    /// </summary>
    public void Update()
    {
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
    /// <returns>
    /// An enumerable containing the keys that are currently pressed.
    /// </returns>
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
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyDown(Keyboard.Key key) => KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key is currently not being pressed.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key is currently up; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyUp(Keyboard.Key key) => !KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was pressed for the first time in the current frame.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key was pressed this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyPressed(Keyboard.Key key) => KeyState[(System.Int32)key] && !PreviousKeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was released for the first time in the current frame.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key was released this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyReleased(Keyboard.Key key) => !KeyState[(System.Int32)key] && PreviousKeyState[(System.Int32)key];

    #endregion Keyboard

    #region Internal Methods

    /// <summary>
    /// Creates a snapshot of the current keyboard state.
    /// </summary>
    /// <returns>
    /// A boolean array representing which keys are down.
    /// </returns>
    internal System.Boolean[] CreateKeyboardStateSnapshot()
    {
        System.Boolean[] arr = new System.Boolean[KeyState.Length];
        KeyState.CopyTo(arr, 0);

        return arr;
    }

    /// <summary>
    /// Restores the keyboard state from a previously saved snapshot.
    /// </summary>
    /// <param name="snapshot">
    /// A boolean array containing key states to restore.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if the provided array length does not match the internal state length.
    /// </exception>
    internal void RestoreKeyboardState(System.Boolean[] snapshot)
    {
        if (snapshot.Length != KeyState.Length)
        {
            NLogixFx.Error(message: $"RestoreKeyboardState: Invalid key state length {snapshot.Length}, expected {KeyState.Length}.", source: "KeyboardManager");
            throw new System.ArgumentException("Invalid key state length");
        }

        for (System.Int32 i = 0; i < KeyState.Length; i++)
        {
            KeyState[i] = snapshot[i];
        }

        NLogixFx.Debug(message: "Keyboard state successfully restored from snapshot.", source: "KeyboardManager");
    }

    #endregion Internal Methods
}