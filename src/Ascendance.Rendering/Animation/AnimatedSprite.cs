// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Animation;

/// <summary>
/// Provides a base class for scene objects that use a sprite and frame-based animation via an <see cref="Animator"/>.
/// </summary>
/// <remarks>
/// (VN) Lớp nền cho mọi object dùng <see cref="Sprite"/> và có hoạt ảnh khung hình.
/// Sử dụng <see cref="PlayAnimationFrames"/> hoặc <see cref="PlayAnimationFromGrid"/> để khởi tạo và chạy hoạt ảnh.
/// </remarks>
public abstract class AnimatedSprite : SpriteObject, System.IDisposable
{
    #region Properties 

    /// <summary>
    /// Gets the <see cref="Animator"/> used to handle sprite animations for this object.
    /// </summary>
    protected readonly Animator SpriteAnimator;

    /// <summary>
    /// Gets a value indicating whether the animation is currently playing.
    /// </summary>
    public System.Boolean IsAnimationPlaying => SpriteAnimator.IsPlaying;

    /// <summary>
    /// Gets the total number of frames in the current animation.
    /// </summary>
    public System.Int32 FrameCount => SpriteAnimator.FrameCount;

    /// <summary>
    /// Gets the index of the current frame.
    /// </summary>
    public System.Int32 CurrentFrameIndex => SpriteAnimator.CurrentFrameIndex;

    #endregion Properties

    #region Construction

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedSprite"/> class with a <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    protected AnimatedSprite(Texture texture)
        : base(texture) => SpriteAnimator = new Animator(Sprite);

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedSprite"/> class with a <see cref="Texture"/> and rectangle.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="rect">The initial rectangle for the texture region.</param>
    protected AnimatedSprite(Texture texture, IntRect rect)
        : base(texture, rect) => SpriteAnimator = new Animator(Sprite);

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedSprite"/> class with a <see cref="Texture"/>, position, scale, and rotation.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="position">The initial position.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="rotation">The initial rotation (degrees).</param>
    protected AnimatedSprite(Texture texture, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, position, scale, rotation) => SpriteAnimator = new Animator(Sprite);

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatedSprite"/> class with a <see cref="Texture"/>, rectangle, position, scale, and rotation.
    /// </summary>
    /// <param name="texture">The sprite texture.</param>
    /// <param name="rect">The initial rectangle for the texture region.</param>
    /// <param name="position">The initial position.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="rotation">The initial rotation (degrees).</param>
    protected AnimatedSprite(Texture texture, IntRect rect, Vector2f position, Vector2f scale, System.Single rotation)
        : base(texture, rect, position, scale, rotation) => SpriteAnimator = new Animator(Sprite);

    #endregion Construction

    #region APIs

    /// <summary>
    /// Starts an animation using the given frames, frame time, and loop setting.
    /// </summary>
    /// <param name="frames">The sequence of sprite-rect frames (draw order).</param>
    /// <param name="frameTime">Duration in seconds for each frame. Must be positive.</param>
    /// <param name="loop">Whether the animation should loop (default: true).</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if <paramref name="frames"/> is null or empty, or if <paramref name="frameTime"/> is not positive.
    /// </exception>
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
    /// Builds animation frames from a uniform grid in the spritesheet and starts the animation.
    /// </summary>
    /// <remarks>
    /// (VN) Dùng khi spritesheet chia đều thành các ô hình chữ nhật nhỏ.
    /// </remarks>
    /// <param name="cellWidth">Width of each cell in pixels.</param>
    /// <param name="cellHeight">Height of each cell in pixels.</param>
    /// <param name="columns">Number of columns in the grid.</param>
    /// <param name="rows">Number of rows in the grid.</param>
    /// <param name="frameTime">Duration in seconds for each frame. Must be positive.</param>
    /// <param name="loop">Whether the animation should loop (default: true).</param>
    /// <param name="startCol">Column index to start from (default: 0).</param>
    /// <param name="startRow">Row index to start from (default: 0).</param>
    /// <param name="count">Optional. The number of frames to use. If not specified, uses the remainder of the grid.</param>
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
    /// Stops the animation and resets it to the first frame.
    /// </summary>
    public void StopAnimation() => SpriteAnimator.Stop();

    /// <summary>
    /// Advances the underlying <see cref="Animator"/> by the specified elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds since the last update.</param>
    public override void Update(System.Single deltaTime) => SpriteAnimator.Update(deltaTime);

    /// <summary>
    /// Releases all resources used by the <see cref="AnimatedSprite"/> instance.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    #endregion APIs

    #region Protected Methods

    /// <summary>
    /// Called when a looping animation wraps from the last frame to the first.
    /// (VN) Ghi đè trong lớp con nếu có nhu cầu.
    /// </summary>
    protected virtual void OnAnimationLooped() { }

    /// <summary>
    /// Called when a non-looping animation reaches its end and stops.
    /// (VN) Ghi đè trong lớp con nếu có nhu cầu.
    /// </summary>
    protected virtual void OnAnimationCompleted() { }

    /// <summary>
    /// Attaches event handlers to the animator's events for looping and completion.
    /// </summary>
    protected void AttachAnimatorEventHandlers()
    {
        this.SpriteAnimator.AnimationLooped += OnAnimationLooped;
        this.SpriteAnimator.AnimationCompleted += OnAnimationCompleted;
    }

    /// <summary>
    /// Releases resources used by the <see cref="AnimatedSprite"/> instance.
    /// </summary>
    /// <param name="disposing">Indicates if the method is called by user code.</param>
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