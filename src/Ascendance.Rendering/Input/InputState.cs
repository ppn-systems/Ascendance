// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Input;

/// <summary>
/// The InputState class provides functionality for handling keyboard and mouse input using SFML.Window.
/// </summary>
public static class InputState
{
    #region Fields

    private static readonly Keyboard.Key[] AllKeys;
    private static readonly Mouse.Button[] AllButtons;

    private static readonly System.Boolean[] KeyState;
    private static readonly System.Boolean[] PreviousKeyState;
    private static readonly System.Boolean[] MouseButtonState;
    private static readonly System.Boolean[] PreviousMouseButtonState;

    private static Vector2i _mousePosition;
    private static System.Int32 _playbackIndex;
    private static System.Boolean _isRecording;
    private static System.Boolean _inputEnabled;
    private static System.Boolean _isPlayingBack;
    private static System.Collections.Generic.List<InputFrame> _recordedInputFrames;
    private static System.Collections.Generic.List<InputFrame> _playbackInputFrames;

    #endregion Fields

    #region Structures

    /// <summary>
    /// Represents the input states for a single frame (for recording/playback).
    /// </summary>
    public class InputFrame
    {
        public System.Boolean[] KeyState;
        public System.Boolean[] MouseButtonState;
        public Vector2i MousePosition;

        public InputFrame Clone()
        {
            return new InputFrame
            {
                KeyState = (System.Boolean[])KeyState.Clone(),
                MouseButtonState = (System.Boolean[])MouseButtonState.Clone(),
                MousePosition = MousePosition
            };
        }
    }

    /// <summary>
    /// Represents an input record (a sequence of input frames).
    /// </summary>
    public class InputRecord
    {
        public System.Collections.Generic.List<InputFrame> Frames = [];
    }

    #endregion Structures

    #region Properties

    /// <summary>
    /// Indicates whether input is currently enabled.
    /// </summary>
    /// <returns>True if input is enabled; otherwise, false.</returns>
    public static System.Boolean IsInputEnabled => _inputEnabled;

    #endregion Properties

    #region Constructor

    static InputState()
    {
        _playbackIndex = 0;
        _isRecording = false;
        _inputEnabled = true;
        _recordedInputFrames = [];
        _playbackInputFrames = null;
        _isPlayingBack = false;

        AllKeys = System.Enum.GetValues<Keyboard.Key>();
        AllButtons = System.Enum.GetValues<Mouse.Button>();
        KeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
        PreviousKeyState = new System.Boolean[(System.Int32)Keyboard.Key.KeyCount];
        MouseButtonState = new System.Boolean[(System.Int32)Mouse.Button.ButtonCount];
        PreviousMouseButtonState = new System.Boolean[(System.Int32)Mouse.Button.ButtonCount];
    }

    #endregion Constructor

    #region Loop

    /// <summary>
    /// Updates the state of all keys and the mouse position. Should be called once per frame.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveOptimization)]
    public static void Update(RenderWindow window)
    {
        if (!_inputEnabled)
        {
            // Clear states if input is blocked
            System.Array.Fill(KeyState, false);
            System.Array.Fill(MouseButtonState, false);
            _mousePosition = new Vector2i(0, 0);
            return;
        }

        if (_isPlayingBack && _playbackInputFrames != null)
        {
            InputFrame frame = _playbackInputFrames[_playbackIndex];
            for (System.Int32 i = 0; i < KeyState.Length; i++)
            {
                PreviousKeyState[i] = KeyState[i];
                KeyState[i] = frame.KeyState[i];
            }

            for (System.Int32 i = 0; i < MouseButtonState.Length; i++)
            {
                PreviousMouseButtonState[i] = MouseButtonState[i];
                MouseButtonState[i] = frame.MouseButtonState[i];
            }

            _mousePosition = frame.MousePosition;
            _playbackIndex = System.Math.Min(_playbackIndex + 1, _playbackInputFrames.Count - 1);
            return;
        }

        // Update key states
        for (System.Int32 i = 0; i < AllKeys.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllKeys[i];
            PreviousKeyState[idx] = KeyState[idx];
            KeyState[idx] = Keyboard.IsKeyPressed(AllKeys[i]);
        }
        // Update mouse button states
        for (System.Int32 i = 0; i < AllButtons.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllButtons[i];
            PreviousMouseButtonState[idx] = MouseButtonState[idx];
            MouseButtonState[idx] = Mouse.IsButtonPressed(AllButtons[i]);
        }

        _mousePosition = Mouse.GetPosition(window);

        if (_isRecording)
        {
            InputFrame frame = new()
            {
                KeyState = (System.Boolean[])KeyState.Clone(),
                MouseButtonState = (System.Boolean[])MouseButtonState.Clone(),
                MousePosition = _mousePosition
            };
            _recordedInputFrames.Add(frame);
        }
    }

    #endregion Loop

    #region API - Input Block

    /// <summary>
    /// Enables all input globally.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void EnableInput() => _inputEnabled = true;

    /// <summary>
    /// Disables all input globally.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void DisableInput() => _inputEnabled = false;

    #endregion API - Input Block

    #region API - Query All Pressed Keys/Buttons

    /// <summary>
    /// Gets all currently pressed keys.
    /// </summary>
    /// <returns>An enumerable containing the pressed keyboard keys.</returns>
    public static System.Collections.Generic.IEnumerable<Keyboard.Key> GetPressedKeys()
    {
        for (System.Int32 i = 0; i < KeyState.Length; i++)
        {
            if (KeyState[i])
            {
                yield return (Keyboard.Key)i;
            }
        }
    }

    /// <summary>
    /// Gets all currently pressed mouse buttons.
    /// </summary>
    /// <returns>An enumerable containing the pressed mouse buttons.</returns>
    public static System.Collections.Generic.IEnumerable<Mouse.Button> GetPressedMouseButtons()
    {
        for (System.Int32 i = 0; i < MouseButtonState.Length; i++)
        {
            if (MouseButtonState[i])
            {
                yield return (Mouse.Button)i;
            }
        }
    }

    #endregion API - Query All Pressed Keys/Buttons

    #region API - Input Recording / Playback

    /// <summary>
    /// Begins recording input frames.
    /// </summary>
    public static void BeginInputRecording()
    {
        _isRecording = true;
        _recordedInputFrames = [];
    }

    /// <summary>
    /// Stops recording and returns the recorded input sequence.
    /// </summary>
    /// <returns>The input record containing all recorded frames.</returns>
    public static InputRecord EndInputRecording()
    {
        _isRecording = false;
        var record = new InputRecord();
        foreach (var frame in _recordedInputFrames)
        {
            record.Frames.Add(frame.Clone());
        }

        return record;
    }

    /// <summary>
    /// Begins playback of a recorded input sequence.
    /// </summary>
    /// <param name="record">The input record to play back.</param>
    public static void BeginInputPlayback(InputRecord record)
    {
        _isPlayingBack = true;
        _playbackInputFrames = record.Frames;
        _playbackIndex = 0;
    }

    /// <summary>
    /// Stops playback and resumes live input collection.
    /// </summary>
    public static void EndInputPlayback()
    {
        _isPlayingBack = false;
        _playbackInputFrames = null;
        _playbackIndex = 0;
    }

    #endregion API - Input Recording / Playback

    #region Getters

    /// <summary>
    /// Gets the current position of the mouse.
    /// </summary>
    /// <returns>ScreenSize tuple containing the X and Y position of the mouse.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Vector2i GetMousePosition() => _mousePosition;

    /// <summary>
    /// Gets the current position of the mouse.
    /// </summary>
    /// <returns>ScreenSize tuple containing the X and Y position of the mouse.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static (System.Single X, System.Single Y) GetMousePositionF() => new(_mousePosition.X, _mousePosition.Y);

    #endregion Getters

    #region Keyboard

    /// <summary>
    /// Checks if a key is currently being pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsKeyDown(Keyboard.Key key) => KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key is currently not being pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is currently up; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsKeyUp(Keyboard.Key key) => !KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was pressed for the first time this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was pressed this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsKeyPressed(Keyboard.Key key) => KeyState[(System.Int32)key] && !PreviousKeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was released for the first time this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was released this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsKeyReleased(Keyboard.Key key) => !KeyState[(System.Int32)key] && PreviousKeyState[(System.Int32)key];

    #endregion Keyboard

    #region Mouse

    /// <summary>
    /// Determines whether the specified mouse button is currently being held down.
    /// </summary>
    /// <param name="button">The mouse button to check (e.g., Left, Right).</param>
    /// <returns>True if the mouse button is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsMouseButtonDown(Mouse.Button button) => MouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just pressed during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button is down now but was not down in the previous frame;
    /// otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsMouseButtonPressed(Mouse.Button button) => MouseButtonState[(System.Int32)button] && !PreviousMouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just released during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button was down in the previous frame but is not down now;
    /// otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsMouseButtonReleased(Mouse.Button button) => !MouseButtonState[(System.Int32)button] && PreviousMouseButtonState[(System.Int32)button];

    #endregion Mouse
}
