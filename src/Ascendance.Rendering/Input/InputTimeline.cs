// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using SFML.System;

namespace Ascendance.Rendering.Input;

/// <summary>
/// Manages recording and playback of input frames for keyboard and mouse events.
/// </summary>
public class InputTimeline : SingletonBase<InputTimeline>
{
    #region Nested Classes

    /// <summary>
    /// Represents the input states for a single frame (for recording or playback).
    /// </summary>
    public class InputFrame
    {
        /// <summary>
        /// The state of all keyboard keys at this frame.
        /// </summary>
        public System.Boolean[] KeyState;

        /// <summary>
        /// The state of all mouse buttons at this frame.
        /// </summary>
        public System.Boolean[] MouseButtonState;

        /// <summary>
        /// The mouse position at this frame.
        /// </summary>
        public Vector2i MousePosition;

        /// <summary>
        /// Creates a deep copy of the current <see cref="InputFrame"/>.
        /// </summary>
        /// <returns>A cloned <see cref="InputFrame"/> instance.</returns>
        public InputFrame Clone()
        {
            return new InputFrame
            {
                KeyState = (System.Boolean[])KeyState?.Clone(),
                MouseButtonState = (System.Boolean[])MouseButtonState?.Clone(),
                MousePosition = MousePosition
            };
        }
    }

    /// <summary>
    /// Represents a recorded sequence of input frames.
    /// </summary>
    public class InputRecord
    {
        /// <summary>
        /// The list of input frames representing this record.
        /// </summary>
        public System.Collections.Generic.List<InputFrame> Frames = [];
    }

    #endregion Nested Classes

    #region Fields

    private System.Collections.Generic.List<InputFrame> _recordedFrames = [];
    private System.Collections.Generic.List<InputFrame> _playbackFrames = [];
    private System.Int32 _playbackIndex = 0;
    private System.Boolean _isRecording = false;
    private System.Boolean _isPlayingBack = false;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets whether the input timeline is currently recording.
    /// </summary>
    public System.Boolean IsRecording => _isRecording;

    /// <summary>
    /// Gets whether the input timeline is currently in playback mode.
    /// </summary>
    public System.Boolean IsPlayingBack => _isPlayingBack;

    #endregion Properties

    #region Recording Methods

    /// <summary>
    /// Starts recording input frames. Previous recordings will be cleared.
    /// </summary>
    public void BeginInputRecording()
    {
        _recordedFrames = [];
        _isRecording = true;
    }

    /// <summary>
    /// Stops recording and returns the recorded input as an <see cref="InputRecord"/>.
    /// </summary>
    /// <returns>An <see cref="InputRecord"/> containing all recorded frames.</returns>
    public InputRecord EndInputRecording()
    {
        _isRecording = false;
        InputRecord record = new();

        foreach (InputFrame frame in _recordedFrames)
        {
            record.Frames.Add(frame.Clone());
        }

        return record;
    }

    #endregion Recording Methods

    #region Playback Methods

    /// <summary>
    /// Begins playback of a previously recorded input sequence.
    /// </summary>
    /// <param name="record">The input record to play back.</param>
    public void BeginInputPlayback(InputRecord record)
    {
        _playbackFrames = new System.Collections.Generic.List<InputFrame>(record.Frames.Count);
        foreach (InputFrame frame in record.Frames)
        {
            _playbackFrames.Add(frame.Clone());
        }

        _playbackIndex = 0;
        _isPlayingBack = true;
    }

    /// <summary>
    /// Ends input playback and resumes live input processing.
    /// </summary>
    public void EndInputPlayback()
    {
        _playbackFrames = [];
        _playbackIndex = 0;
        _isPlayingBack = false;
    }

    #endregion Playback Methods

    #region Runtime Update

    /// <summary>
    /// Updates the input timeline.
    /// Should be called once per frame (typically from the main update loop).
    /// </summary>
    public void Update()
    {
        if (_isRecording)
        {
            // Record the current frame input state
            InputFrame frame = new()
            {
                KeyState = KeyboardManager.Instance.CreateKeyboardStateSnapshot(),
                MouseButtonState = MouseManager.Instance.CreateMouseButtonSnapshot(),
                MousePosition = MouseManager.Instance.GetMousePosition()
            };
            _recordedFrames.Add(frame);
        }
        else if (_isPlayingBack && _playbackFrames.Count > 0)
        {
            // Playback the recorded input frame
            if (_playbackIndex >= _playbackFrames.Count)
            {
                _playbackIndex = _playbackFrames.Count - 1;
            }

            InputFrame current = _playbackFrames[_playbackIndex];
            KeyboardManager.Instance.RestoreKeyboardState(current.KeyState);
            MouseManager.Instance.RestoreMouseState(current.MouseButtonState, current.MousePosition);

            _playbackIndex++;
            if (_playbackIndex >= _playbackFrames.Count)
            {
                // Optional: call EndInputPlayback automatically if you want playback to end when finished
                // EndInputPlayback();
            }
        }
    }

    #endregion Runtime Update
}