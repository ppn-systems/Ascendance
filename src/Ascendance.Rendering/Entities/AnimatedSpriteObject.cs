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
public abstract class AnimatedSpriteObject : SpriteObject
{
    #region Properties 

    /// <summary>
    /// The <see cref="Animator"/> used to handle sprite animations.
    /// </summary>
    protected readonly Animator Animator;

    /// <summary>
    /// Indicates if the animator is currently playing.
    /// </summary>
    public System.Boolean IsPlaying => Animator.Playing;

    /// <summary>
    /// Index of the current frame.
    /// </summary>
    public System.Int32 CurrentFrameIndex => Animator.CurrentFrameIndex;

    /// <summary>
    /// Number of frames in the current animation.
    /// </summary>
    public System.Int32 FrameCount => Animator.FrameCount;

    #endregion Properties

    #region Construction

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture)
        : base(texture) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, IntRect rect)
        : base(texture, rect) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, position, scale, rotation) => Animator = new Animator(Sprite);

    /// <inheritdoc/>
    protected AnimatedSpriteObject(Texture texture, IntRect rect, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, rect, position, scale, rotation) => Animator = new Animator(Sprite);

    #endregion Construction

    #region APIs

    /// <summary>
    /// Starts an animation with the given frames, frame time and loop setting.
    /// </summary>
    /// <param name="frames">Sequence of sprite-rect frames (draw order).</param>
    /// <param name="frameTime">Seconds per frame.</param>
    /// <param name="loop">Whether the animation should loop.</param>
    /// <exception cref="System.ArgumentException">If frameTime is not positive.</exception>
    public void SetAnimationFrames(
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

        Animator.SetFrames(frames);
        Animator.SetFrameTime(frameTime);
        Animator.Loop = loop;
        Animator.Play();
    }

    /// <summary>
    /// Convenience overload: builds frames from a grid and starts playing.
    /// </summary>
    /// <remarks>(VN) Dùng khi spritesheet chia ô đều nhau.</remarks>
    public void SetAnimationFromGrid(
        System.Int32 cellWidth, System.Int32 cellHeight,
        System.Int32 columns, System.Int32 rows,
        System.Single frameTime,
        System.Boolean loop = true,
        System.Int32 startCol = 0, System.Int32 startRow = 0,
        System.Int32? count = null)
    {
        Animator.BuildGridFrames(cellWidth, cellHeight, columns, rows, startCol, startRow, count);
        Animator.SetFrameTime(frameTime);
        Animator.Loop = loop;
        Animator.Play();
    }

    /// <summary>
    /// Plays (or resumes) the current animation.
    /// </summary>
    public void Play() => Animator.Play();

    /// <summary>
    /// Pauses the current animation.
    /// </summary>
    public new void Pause() => Animator.Pause();

    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    public void Stop() => Animator.Stop();

    /// <summary>
    /// Advances the bound <see cref="Animator"/> by <paramref name="deltaTime"/>.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last update.</param>
    public override void Update(System.Single deltaTime)
        => Animator.Update(deltaTime);

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

    protected void HookAnimatorEvents()
    {
        Animator.OnLooped += OnAnimationLooped;
        Animator.OnCompleted += OnAnimationCompleted;
    }

    #endregion APIs
}