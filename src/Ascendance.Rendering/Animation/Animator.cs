// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Enums;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using System.Collections.Generic;

namespace Ascendance.Rendering.Animation;

/// <summary>
/// Handles advanced frame-based animation for a SFML <see cref="Sprite"/> using a list of <see cref="IntRect"/> frames.
/// Supports state tracking, frame events, reverse playback, and more.
/// </summary>
public sealed class Animator : IUpdatable, System.IDisposable
{
    #region Fields

    private readonly Sprite _sprite;
    private readonly List<IntRect> _frames = [];
    private System.Int32 _index;          // Current frame index
    private System.Single _frameTime = 0.1f;    // Seconds per frame
    private System.Single _accumulator;
    private System.Boolean _reverse;

    #endregion

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
    /// Raised each time the frame index changes.
    /// </summary>
    public event System.Action<System.Int32> FrameChanged;

    #endregion

    #region Properties

    public AnimationState State { get; private set; } = AnimationState.Idle;

    /// <summary>
    /// Gets or sets a value indicating whether the animation should loop when it reaches the last frame.
    /// </summary>
    public System.Boolean Loop { get; set; } = true;

    /// <summary>
    /// True if animation is actively playing (State==Playing).
    /// </summary>
    public System.Boolean IsPlaying => State == AnimationState.Playing;

    /// <summary>
    /// Total frames.
    /// </summary>
    public System.Int32 FrameCount => _frames.Count;

    /// <summary>
    /// Chỉ số khung hình hiện tại (zero-based), trả về -1 nếu trống.
    /// </summary>
    public System.Int32 CurrentFrameIndex => _frames.Count == 0 ? -1 : _index;

    /// <summary>
    /// Trả về true nếu không có frame nào.
    /// </summary>
    public System.Boolean IsEmpty => _frames.Count == 0;

    /// <summary>
    /// Thời gian mỗi frame (giây), tối thiểu 0.001.
    /// </summary>
    public System.Single FrameTime
    {
        get => _frameTime;
        set => _frameTime = System.MathF.Max(0.001f, value);
    }

    /// <summary>
    /// Playback in reverse if true.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "<Pending>")]
    public System.Boolean Reverse
    {
        get => _reverse;
        set => _reverse = value;
    }

    #endregion

    #region Construction

    public Animator(Sprite sprite, System.Single frameTime = 0.1f)
    {
        _sprite = sprite ?? throw new System.ArgumentNullException(nameof(sprite));
        FrameTime = frameTime;
    }

    #endregion

    #region APIs

    /// <summary>
    /// Đổi toàn bộ frame của hoạt ảnh và reset về đầu.
    /// </summary>
    public void SetFrames(IReadOnlyList<IntRect> frames)
    {
        _frames.Clear();
        if (frames is not null)
        {
            _frames.AddRange(frames);
        }

        RESET_TO_FIRST_FRAME();
        APPLY_FRAME();
    }

    /// <summary>
    /// Add 1 frame cuối danh sách.
    /// </summary>
    public void AddFrame(IntRect frame) => _frames.Add(frame);

    /// <summary>
    /// Add nhiều frame cuối danh sách.
    /// </summary>
    public void AddFrames(IEnumerable<IntRect> frames)
    {
        if (frames != null)
        {
            _frames.AddRange(frames);
        }
    }

    /// <summary>
    /// Xóa toàn bộ frame.
    /// </summary>
    public void ClearFrames()
    {
        _frames.Clear();
        Stop();
    }

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
        APPLY_FRAME();
        if (_index != prev)
        {
            FrameChanged?.Invoke(_index);
        }
    }

    /// <summary>
    /// Next frame (dừng nếu cuối, hoặc loop nếu Loop=true). Không thay đổi accumulator.
    /// </summary>
    public void NextFrame()
    {
        if (_frames.Count < 2)
        {
            return;
        }

        if (_index + 1 >= _frames.Count)
        {
            GoToFrame(Loop ? 0 : _index);
        }
        else
        {
            GoToFrame(_index + 1);
        }
    }

    /// <summary>
    /// Previous frame.
    /// </summary>
    public void PrevFrame()
    {
        if (_frames.Count < 2)
        {
            return;
        }

        if (_index < 1)
        {
            GoToFrame(Loop ? _frames.Count - 1 : _index);
        }
        else
        {
            GoToFrame(_index - 1);
        }
    }

    public void Play()
    {
        if (_frames.Count == 0)
        {
            RESET_TO_FIRST_FRAME();
            State = AnimationState.Idle;
            return;
        }
        if (State != AnimationState.Playing)
        {
            State = AnimationState.Playing;
        }
    }

    public void Pause()
    {
        if (State == AnimationState.Playing)
        {
            State = AnimationState.Paused;
        }
    }

    public void Stop()
    {
        State = AnimationState.Stopped;
        RESET_TO_FIRST_FRAME();
        APPLY_FRAME();
    }

    public void Resume()
    {
        if (State == AnimationState.Paused)
        {
            State = AnimationState.Playing;
        }
    }

    public void SetFrameTime(System.Single seconds) => FrameTime = seconds;

    /// <summary>
    /// Lấy bản sao readonly frames.
    /// </summary>
    public IReadOnlyList<IntRect> GetFramesReadonly() => _frames.AsReadOnly();

    /// <summary>
    /// Tạo frame dạng lưới (cho spritesheet phổ thông).
    /// </summary>
    public void BuildGridFrames(
        System.Int32 cellWidth, System.Int32 cellHeight,
        System.Int32 columns, System.Int32 rows,
        System.Int32 startCol = 0, System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        List<IntRect> list = [];
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
    /// Cập nhật theo thời gian trôi qua (deltaTime - giây).
    /// </summary>
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
                    APPLY_FRAME();
                    AnimationLooped?.Invoke();
                }
                else
                {
                    _index = _reverse ? 0 : _frames.Count - 1;
                    APPLY_FRAME();
                    State = AnimationState.Stopped;
                    AnimationCompleted?.Invoke();
                    break;
                }
            }
            else
            {
                _index = System.Math.Clamp(next, 0, _frames.Count - 1);
                APPLY_FRAME();
                if (_index != prev)
                {
                    FrameChanged?.Invoke(_index);
                }
            }
        }
    }

    #endregion

    #region Private Helpers

    private void RESET_TO_FIRST_FRAME()
    {
        _index = _reverse ? _frames.Count - 1 : 0;
        _accumulator = 0f;
    }

    private void APPLY_FRAME()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        _sprite.TextureRect = _frames[_index];
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        AnimationCompleted = null;
        AnimationLooped = null;
        FrameChanged = null;
    }

    #endregion
}