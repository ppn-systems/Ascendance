// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Enums;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;

namespace Ascendance.Rendering.Animation;

/// <summary>
/// Handles advanced frame-based animation for an SFML <see cref="Sprite"/> using a list of <see cref="IntRect"/> frames.
/// Supports state tracking, frame events, reverse playback, and more.
/// </summary>
public sealed class Animator : IUpdatable, System.IDisposable
{
    #region Fields

    private readonly Sprite _sprite;
    private readonly System.Collections.Generic.List<IntRect> _frames = [];

    private System.Int32 _index;
    private System.Boolean _reverse;
    private System.Single _frameTime;
    private System.Single _accumulator;

    #endregion Fields

    #region Events

    /// <summary>
    /// Occurs when the animation completes (when <see cref="Loop"/> is false).
    /// </summary>
    public event System.Action AnimationCompleted;

    /// <summary>
    /// Occurs when the animation loops from the last frame to the first frame.
    /// </summary>
    public event System.Action AnimationLooped;

    /// <summary>
    /// Occurs whenever the frame index changes.
    /// </summary>
    public event System.Action<System.Int32> FrameChanged;

    #endregion Events

    #region Properties

    /// <summary>
    /// Gets the current animation state.
    /// </summary>
    public AnimationState State { get; private set; } = AnimationState.Idle;

    /// <summary>
    /// Gets or sets a value indicating whether the animation should loop when it reaches the last frame.
    /// </summary>
    public System.Boolean Loop { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the animation is currently playing (<see cref="AnimationState.Playing"/>).
    /// </summary>
    public System.Boolean IsPlaying => State == AnimationState.Playing;

    /// <summary>
    /// Gets the total number of frames.
    /// </summary>
    public System.Int32 FrameCount => _frames.Count;

    /// <summary>
    /// Gets the current frame index (zero-based). Returns -1 if there are no frames.
    /// </summary>
    public System.Int32 CurrentFrameIndex => _frames.Count == 0 ? -1 : _index;

    /// <summary>
    /// Gets a value indicating whether there are no frames in the animation.
    /// </summary>
    public System.Boolean IsEmpty => _frames.Count == 0;

    /// <summary>
    /// Gets or sets the duration (in seconds) for each frame. Minimum value is 0.001.
    /// </summary>
    public System.Single FrameTime
    {
        get => _frameTime;
        set => _frameTime = System.MathF.Max(0.001f, value);
    }

    /// <summary>
    /// Gets or sets whether playback occurs in reverse.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "<Pending>")]
    public System.Boolean Reverse
    {
        get => _reverse;
        set => _reverse = value;
    }

    #endregion Properties

    #region Construction

    /// <summary>
    /// Initializes a new instance of the <see cref="Animator"/> class for the specified <see cref="Sprite"/>.
    /// </summary>
    /// <param name="sprite">The sprite to animate.</param>
    /// <param name="frameTime">The duration (in seconds) per frame.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="sprite"/> is null.</exception>
    public Animator(Sprite sprite, System.Single frameTime = 0.1f)
    {
        _sprite = sprite ?? throw new System.ArgumentNullException(nameof(sprite));

        _frameTime = 0.1f;
        this.FrameTime = frameTime;
    }

    #endregion Construction

    #region APIs

    /// <summary>
    /// Replaces all frames of the animation and resets to the first frame.
    /// </summary>
    /// <param name="frames">Read-only list of frame rectangles.</param>
    public void SetFrames(System.Collections.Generic.IReadOnlyList<IntRect> frames)
    {
        _frames.Clear();
        if (frames is not null)
        {
            _frames.AddRange(frames);
        }

        this.RESET_TO_FIRST_FRAME();
        this.APPLYF_RAME();
    }

    /// <summary>
    /// Adds a frame to the end of the frame list.
    /// </summary>
    /// <param name="frame">Frame rectangle to add.</param>
    public void AddFrame(IntRect frame) => _frames.Add(frame);

    /// <summary>
    /// Adds multiple frames to the end of the frame list.
    /// </summary>
    /// <param name="frames">Enumerable collection of frame rectangles to add.</param>
    public void AddFrames(System.Collections.Generic.IEnumerable<IntRect> frames)
    {
        if (frames != null)
        {
            _frames.AddRange(frames);
        }
    }

    /// <summary>
    /// Removes all frames from the animation.
    /// </summary>
    public void ClearFrames()
    {
        _frames.Clear();
        this.Stop();
    }

    /// <summary>
    /// Moves to the specified frame index.
    /// </summary>
    /// <param name="index">Zero-based frame index.</param>
    public void GoToFrame(System.Int32 index)
    {
        if (_frames.Count == 0)
        {
            _index = 0;
            return;
        }
        System.Int32 prev = _index;
        _index = System.Math.Clamp(index, 0, _frames.Count - 1);
        _accumulator = 0f;
        this.APPLYF_RAME();
        if (_index != prev)
        {
            FrameChanged?.Invoke(_index);
        }
    }

    /// <summary>
    /// Advances to the next frame. If at the last frame, stops or loops (depending on <see cref="Loop"/>).
    /// Does not affect accumulated time.
    /// </summary>
    public void NextFrame()
    {
        if (_frames.Count < 2)
        {
            return;
        }

        if (_index + 1 >= _frames.Count)
        {
            this.GoToFrame(Loop ? 0 : _index);
        }
        else
        {
            this.GoToFrame(_index + 1);
        }
    }

    /// <summary>
    /// Moves to the previous frame. Loops if at the first frame (if <see cref="Loop"/> is true).
    /// </summary>
    public void PrevFrame()
    {
        if (_frames.Count < 2)
        {
            return;
        }

        if (_index < 1)
        {
            this.GoToFrame(Loop ? _frames.Count - 1 : _index);
        }
        else
        {
            this.GoToFrame(_index - 1);
        }
    }

    /// <summary>
    /// Starts playback from the current frame.
    /// </summary>
    public void Play()
    {
        if (_frames.Count == 0)
        {
            this.RESET_TO_FIRST_FRAME();
            this.State = AnimationState.Idle;
            return;
        }
        if (this.State != AnimationState.Playing)
        {
            this.State = AnimationState.Playing;
        }
    }

    /// <summary>
    /// Pauses the animation at the current frame.
    /// </summary>
    public void Pause()
    {
        if (this.State == AnimationState.Playing)
        {
            this.State = AnimationState.Paused;
        }
    }

    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    public void Stop()
    {
        this.State = AnimationState.Stopped;
        this.RESET_TO_FIRST_FRAME();
        this.APPLYF_RAME();
    }

    /// <summary>
    /// Resumes playback if paused.
    /// </summary>
    public void Resume()
    {
        if (this.State == AnimationState.Paused)
        {
            this.State = AnimationState.Playing;
        }
    }

    /// <summary>
    /// Sets the duration (in seconds) per frame.
    /// </summary>
    /// <param name="seconds">Duration in seconds. Must be positive.</param>
    public void SetFrameTime(System.Single seconds) => this.FrameTime = seconds;

    /// <summary>
    /// Gets a read-only copy of the frame list.
    /// </summary>
    /// <returns>A read-only list of frame rectangles.</returns>
    public System.Collections.Generic.IReadOnlyList<IntRect> GetFramesReadonly() => _frames.AsReadOnly();

    /// <summary>
    /// Builds a grid of frames (for standard spritesheets) and sets them as the current animation frame set.
    /// </summary>
    /// <param name="cellWidth">Width of each cell in pixels.</param>
    /// <param name="cellHeight">Height of each cell in pixels.</param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <param name="rows">Number of rows in the grid.</param>
    /// <param name="startCol">Starting column index. Defaults to 0.</param>
    /// <param name="startRow">Starting row index. Defaults to 0.</param>
    /// <param name="count">Optional: number of frames to use. If not specified, uses all frames from the start position.</param>
    public void BuildGridFrames(
        System.Int32 cellWidth, System.Int32 cellHeight,
        System.Int32 columns, System.Int32 rows,
        System.Int32 startCol = 0, System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        System.Collections.Generic.List<IntRect> list = [];
        System.Int32 total = columns * rows;
        System.Int32 start = (startRow * columns) + startCol;
        System.Int32 take = count ?? (total - start);

        for (System.Int32 k = 0; k < take; k++)
        {
            System.Int32 id = start + k;
            if (id >= total)
            {
                break;
            }

            System.Int32 r = id / columns;
            System.Int32 c = id % columns;
            list.Add(new IntRect(c * cellWidth, r * cellHeight, cellWidth, cellHeight));
        }

        this.SetFrames(list);
    }

    /// <summary>
    /// Updates the animation state by the specified elapsed time (in seconds).
    /// </summary>
    /// <param name="deltaTime">Amount of time to update by (in seconds).</param>
    public void Update(System.Single deltaTime)
    {
        if (State != AnimationState.Playing || _frames.Count == 0)
        {
            return;
        }

        _accumulator += deltaTime;
        while (_accumulator >= _frameTime)
        {
            _accumulator -= _frameTime;

            System.Int32 prev = _index;
            System.Int32 next = _reverse ? _index - 1 : _index + 1;

            System.Boolean rewinding = _reverse && next < 0;
            System.Boolean reachingEnd = !_reverse && next >= _frames.Count;

            if (rewinding || reachingEnd)
            {
                if (Loop)
                {
                    _index = _reverse ? _frames.Count - 1 : 0;
                    this.APPLYF_RAME();
                    this.AnimationLooped?.Invoke();
                }
                else
                {
                    _index = _reverse ? 0 : _frames.Count - 1;
                    this.APPLYF_RAME();
                    this.State = AnimationState.Stopped;
                    this.AnimationCompleted?.Invoke();
                    break;
                }
            }
            else
            {
                _index = System.Math.Clamp(next, 0, _frames.Count - 1);
                this.APPLYF_RAME();
                if (_index != prev)
                {
                    this.FrameChanged?.Invoke(_index);
                }
            }
        }
    }

    #endregion APIs

    #region IDisposable

    /// <summary>
    /// Unsubscribes all event handlers and releases resources.
    /// </summary>
    public void Dispose()
    {
        AnimationCompleted = null;
        AnimationLooped = null;
        FrameChanged = null;
    }

    #endregion IDisposable

    #region Private Helpers

    private void RESET_TO_FIRST_FRAME()
    {
        _index = _reverse ? _frames.Count - 1 : 0;
        _accumulator = 0f;
    }

    private void APPLYF_RAME()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        _sprite.TextureRect = _frames[_index];
    }

    #endregion Private Helpers
}