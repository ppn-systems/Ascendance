// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Animation;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Entities;

/// <summary>
/// Base class for sprite-based scene objects using a spritesheet animator.
/// </summary>
/// <remarks>
/// (VN) Lớp nền cho mọi object dùng Sprite và có hoạt ảnh khung hình.
/// Dùng SetAnimationFrames hoặc SetAnimationFromGrid để khởi tạo và chạy animation.
/// </remarks>
public abstract class AnimatorObject : SpriteObject, System.IDisposable
{
    #region Properties 

    /// <summary>
    /// The <see cref="SpriteAnimator"/> used to handle sprite animations.
    /// </summary>
    protected readonly Animator SpriteAnimator;

    /// <summary>
    /// Indicates if the animator is currently playing.
    /// </summary>
    public System.Boolean IsAnimationPlaying => SpriteAnimator.IsPlaying;

    /// <summary>
    /// Number of frames in the current animation.
    /// </summary>
    public System.Int32 FrameCount => SpriteAnimator.FrameCount;

    /// <summary>
    /// Index of the current frame.
    /// </summary>
    public System.Int32 CurrentFrameIndex => SpriteAnimator.CurrentFrameIndex;

    #endregion Properties

    #region Construction

    /// <inheritdoc/>
    protected AnimatorObject(Texture texture)
        : base(texture) => SpriteAnimator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatorObject(Texture texture, IntRect rect)
        : base(texture, rect) => SpriteAnimator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatorObject(Texture texture, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, position, scale, rotation) => SpriteAnimator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatorObject(Texture texture, IntRect rect, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, rect, position, scale, rotation) => SpriteAnimator = new Animator(Sprite);

    #endregion Construction

    #region APIs

    /// <summary>
    /// Starts an animation with the given frames, frame time and loop setting.
    /// </summary>
    /// <param name="frames">Sequence of sprite-rect frames (draw order).</param>
    /// <param name="frameTime">Seconds per frame.</param>
    /// <param name="loop">Whether the animation should loop.</param>
    /// <exception cref="System.ArgumentException">If frameTime is not positive.</exception>
    public void PlayAnimationFrames(
        System.Collections.Generic.IReadOnlyList<IntRect> frames,
        System.Single frameTime, System.Boolean loop = true)
    {
        if (frames == null || frames.Count == 0)
        {
            throw new System.ArgumentException("Frames list must not be null or empty.", nameof(frames));
        }

        if (frameTime <= 0)
        {
            throw new System.ArgumentException("FrameTime must be positive.", nameof(frameTime));
        }

        SpriteAnimator.SetFrames(frames);
        SpriteAnimator.SetFrameTime(frameTime);
        SpriteAnimator.Loop = loop;
        SpriteAnimator.Play();
    }

    /// <summary>
    /// Convenience overload: builds frames from a grid and starts playing.
    /// </summary>
    /// <remarks>(VN) Dùng khi spritesheet chia ô đều nhau.</remarks>
    public void PlayAnimationFromGrid(
        System.Int32 cellWidth, System.Int32 cellHeight,
        System.Int32 columns, System.Int32 rows,
        System.Single frameTime,
        System.Boolean loop = true,
        System.Int32 startCol = 0, System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        SpriteAnimator.BuildGridFrames(cellWidth, cellHeight, columns, rows, startCol, startRow, count);
        SpriteAnimator.SetFrameTime(frameTime);
        SpriteAnimator.Loop = loop;
        SpriteAnimator.Play();
    }


    /// <summary>
    /// Plays (or resumes) the current animation.
    /// </summary>
    public void PlayAnimation() => SpriteAnimator.Play();

    /// <summary>
    /// Pauses the current animation.
    /// </summary>
    public void PauseAnimation() => SpriteAnimator.Pause();

    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    public void StopAnimation() => SpriteAnimator.Stop();

    /// <summary>
    /// Advances the bound <see cref="SpriteAnimator"/> by <paramref name="deltaTime"/>.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last update.</param>
    public override void Update(System.Single deltaTime) => SpriteAnimator.Update(deltaTime);

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    #endregion APIs

    #region Protected Methods

    /// <summary>
    /// Called when a looping animation wraps from last frame to first.
    /// (VN) Override trong lớp con nếu cần.
    /// </summary>
    protected virtual void OnAnimationLooped() { }

    /// <summary>
    /// Called when a non-looping animation reaches its end and stops.
    /// (VN) Override trong lớp con nếu cần.
    /// </summary>
    protected virtual void OnAnimationCompleted() { }

    /// <summary>
    /// Attaches event handlers to the animator's events.
    /// </summary>
    protected void AttachAnimatorEventHandlers()
    {
        this.SpriteAnimator.AnimationLooped += OnAnimationLooped;
        this.SpriteAnimator.AnimationCompleted += OnAnimationCompleted;
    }

    /// <summary>
    /// Disposes the object and its resources.
    /// </summary>
    protected virtual void Dispose(System.Boolean disposing)
    {
        if (disposing)
        {
            this.SpriteAnimator.AnimationLooped -= OnAnimationLooped;
            this.SpriteAnimator.AnimationCompleted -= OnAnimationCompleted;
        }
    }

    #endregion Protected Methods
}