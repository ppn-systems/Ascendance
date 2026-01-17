// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;
using SFML.Graphics;

namespace Ascendance.Rendering.Animation;

/// <summary>
/// Handles frame-based animation for a SFML Sprite using an IntRect frame list.
/// </summary>
/// <remarks>
/// (VN) Quản lý hoạt ảnh theo khung hình (spritesheet). Gọi Update mỗi frame để tiến thời gian.
/// Dùng OnLooped, OnCompleted event để bắt trạng thái kết thúc.
/// </remarks>
public sealed class Animator : IRenderUpdatable
{
    #region Fields

    private readonly Sprite _sprite;
    private readonly System.Collections.Generic.List<IntRect> _frames = [];

    private System.Int32 _index;          // current frame index
    private System.Single _frameTime;     // seconds per frame
    private System.Single _accumulator;   // accumulated time since last advance

    #endregion Fields

    #region Properties

    /// <summary>Event fired when animation completes (not looping).</summary>
    public event System.Action AnimationCompleted;

    /// <summary>Event fired when animation loops from last frame to first.</summary>
    public event System.Action AnimationLooped;

    /// <summary>Whether the animation should loop on completion.</summary>
    public System.Boolean Loop { get; set; } = true;

    /// <summary>Whether the animation is currently playing.</summary>
    public System.Boolean IsPlaying { get; private set; }

    /// <summary>Total number of frames.</summary>
    public System.Int32 FrameCount => _frames.Count;

    /// <summary>Current frame index (0-based). -1 if no frame.</summary>
    public System.Int32 CurrentFrameIndex => _frames.Count == 0 ? -1 : _index;

    /// <summary>True if not frames are set.</summary>
    public System.Boolean IsEmpty => _frames.Count == 0;

    /// <summary>Seconds per frame. Min = 0.001s.</summary>
    public System.Single FrameTime
    {
        get => _frameTime;
        set => _frameTime = System.MathF.Max(0.001f, value);
    }

    #endregion Properties

    #region Construction

    /// <summary>
    /// Creates a new animator bound to a Sprite.
    /// </summary>
    /// <param name="sprite">Target Sprite.</param>
    /// <param name="frameTime">Seconds per frame.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Animator(Sprite sprite, System.Single frameTime = 0.1f)
    {
        _frames = [];
        _sprite = sprite ?? throw new System.ArgumentNullException(nameof(sprite));
        _frameTime = System.MathF.Max(0.001f, frameTime);
    }

    #endregion Construction

    #region APIs

    /// <summary>Replaces all frames and resets to the first frame.</summary>
    public void SetFrames(System.Collections.Generic.IReadOnlyList<IntRect> frames)
    {
        _frames.Clear();
        if (frames != null)
        {
            _frames.AddRange(frames);
        }

        ResetToFirstFrame();
        ApplyFrame();
    }

    /// <summary>Adds a single frame to the end.</summary>
    public void AddFrame(IntRect frame) => _frames.Add(frame);

    /// <summary>Adds multiple frames to the end.</summary>
    public void AddFrames(System.Collections.Generic.IEnumerable<IntRect> frames)
    {
        if (frames != null)
        {
            _frames.AddRange(frames);
        }
    }

    /// <summary>Clears all frames and stops animation.</summary>
    public void ClearFrames()
    {
        _frames.Clear();
        Stop();
    }

    /// <summary>Go to a specific frame index immediately.</summary>
    /// <param name="index">Frame index (0-based).</param>
    public void GoToFrame(System.Int32 index)
    {
        if (_frames.Count == 0)
        {
            _index = 0; return;
        }

        _index = System.Math.Clamp(index, 0, _frames.Count - 1);
        _accumulator = 0f;
        ApplyFrame();
    }

    /// <summary>
    /// Builds frames from a grid (rows x cols) in a spritesheet.
    /// </summary>
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
        SetFrames(list);
    }

    /// <summary>
    /// Start advancing frames.
    /// </summary>
    public void Play() => IsPlaying = true;

    /// <summary>
    /// Pause advancing frames.
    /// </summary>
    public void Pause() => IsPlaying = false;

    /// <summary>
    /// Stop and reset to first frame.
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        ResetToFirstFrame();
        ApplyFrame();
    }

    /// <summary>
    /// Set seconds per frame.
    /// </summary>
    public void SetFrameTime(System.Single seconds) => FrameTime = seconds;

    /// <summary>
    /// Advance animation by deltaTime seconds.
    /// </summary>
    public void Update(System.Single deltaTime)
    {
        if (!IsPlaying || _frames.Count == 0)
        {
            return;
        }

        _accumulator += deltaTime;
        while (_accumulator >= _frameTime)
        {
            _accumulator -= _frameTime;
            System.Int32 next = _index + 1;

            if (next >= _frames.Count)
            {
                if (Loop)
                {
                    next = 0;
                    AnimationLooped?.Invoke();
                }
                else
                {
                    _index = _frames.Count - 1;
                    ApplyFrame();
                    IsPlaying = false;
                    AnimationCompleted?.Invoke();
                    break;
                }
            }
            _index = next;
            ApplyFrame();
        }
    }


    #endregion APIs

    #region Private Methods

    private void ResetToFirstFrame()
    {
        _index = 0;
        _accumulator = 0f;
    }

    private void ApplyFrame()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        _sprite.TextureRect = _frames[_index];
    }

    #endregion Private Methods
}