// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using SFML.System;

namespace Ascendance.Rendering.Input;

/// <summary>
/// Provides a timeline-based system for recording and replaying
/// keyboard and mouse input on a per-frame basis.
/// Commonly used for input replay, debugging, testing, or deterministic simulations.
/// </summary>
public class InputTimeline : SingletonBase<InputTimeline>
{
    #region Nested Classes

    /// <summary>
    /// Represents the complete input state captured for a single frame.
    /// </summary>
    public class InputFrame
    {
        /// <summary>
        /// Gets or sets the keyboard key states for this frame.
        /// Each index corresponds to a specific key.
        /// </summary>
        public System.Boolean[] KeyState;

        /// <summary>
        /// Gets or sets the mouse button states for this frame.
        /// Each index corresponds to a specific mouse button.
        /// </summary>
        public System.Boolean[] MouseButtonState;

        /// <summary>
        /// Gets or sets the mouse cursor position for this frame.
        /// </summary>
        public Vector2i MousePosition;

        /// <summary>
        /// Creates a deep copy of this <see cref="InputFrame"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="InputFrame"/> instance containing cloned input state data.
        /// </returns>
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
    /// Represents a full recording session consisting of multiple input frames.
    /// </summary>
    public class InputRecord
    {
        /// <summary>
        /// Gets the list of recorded input frames in chronological order.
        /// </summary>
        public System.Collections.Generic.List<InputFrame> Frames = [];
    }

    #endregion Nested Classes

    #region Fields

    private System.Int32 _playbackIndex;
    private System.Boolean _isRecording;
    private System.Boolean _isPlayingBack;
    private System.Collections.Generic.List<InputFrame> _recordedFrames = [];
    private System.Collections.Generic.List<InputFrame> _playbackFrames = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the system is currently recording input.
    /// </summary>
    public System.Boolean IsRecording => _isRecording;

    /// <summary>
    /// Gets a value indicating whether the system is currently replaying recorded input.
    /// </summary>
    public System.Boolean IsPlayingBack => _isPlayingBack;

    #endregion Properties

    #region Recording Methods

    /// <summary>
    /// Begins capturing input data on a per-frame basis.
    /// Any previously recorded input will be discarded.
    /// </summary>
    public void START_RECORDING()
    {
        _recordedFrames = [];
        _isRecording = true;
    }

    /// <summary>
    /// Stops input recording and packages the captured frames into an <see cref="InputRecord"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="InputRecord"/> containing all recorded input frames.
    /// </returns>
    public InputRecord STOP_RECORDING()
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
    /// Starts replaying a previously recorded input sequence.
    /// </summary>
    /// <param name="record">
    /// The input record containing frames to replay.
    /// </param>
    public void START_PLAY_BACK(InputRecord record)
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
    /// Stops input playback and clears all playback state.
    /// Live input processing will resume.
    /// </summary>
    public void STOP_PLAY_BACK()
    {
        _playbackIndex = 0;
        _playbackFrames = [];
        _isPlayingBack = false;
    }

    #endregion Playback Methods

    #region Runtime Update

    /// <summary>
    /// Updates the input timeline state.
    /// This method should be called once per frame from the main update loop.
    /// </summary>
    public void Update()
    {
        if (_isRecording)
        {
            // Capture current input state
            InputFrame frame = new()
            {
                MousePosition = MouseManager.Instance.GetMousePosition(),
                KeyState = KeyboardManager.Instance.CreateKeyboardStateSnapshot(),
                MouseButtonState = MouseManager.Instance.CreateMouseButtonSnapshot()
            };

            _recordedFrames.Add(frame);
        }
        else if (_isPlayingBack && _playbackFrames.Count > 0)
        {
            if (_playbackIndex >= _playbackFrames.Count)
            {
                _playbackIndex = _playbackFrames.Count - 1;
            }

            InputFrame current = _playbackFrames[_playbackIndex];
            KeyboardManager.Instance.RestoreKeyboardState(current.KeyState);
            MouseManager.Instance.RestoreMouseState(current.MouseButtonState, current.MousePosition);

            _playbackIndex++;
        }
    }

    #endregion Runtime Update
}
