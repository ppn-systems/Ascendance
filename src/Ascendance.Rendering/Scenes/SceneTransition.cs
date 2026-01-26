// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Scenes.Effects;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;

namespace Ascendance.Rendering.Scenes;

/// <summary>
/// Runs a two-phase scene transition via a full-screen overlay: <b>closing</b> (cover) → <b>switch scene</b> → <b>opening</b> (reveal).
/// (VN) Hiệu ứng chuyển cảnh 2 pha (đóng → đổi cảnh → mở) thông qua một lớp overlay hiệu ứng.
/// </summary>
/// <remarks>
/// Instance này tồn tại xuyên cảnh và tự hủy khi hoàn tất.
/// Dùng hiệu ứng vẽ được chọn qua <see cref="ITransitionDrawable"/> strategy.
/// </remarks>
public sealed class SceneTransition : RenderObject, IUpdatable
{
    #region Fields

    private readonly ITransitionDrawable _effect;
    private readonly System.String _nextSceneName;
    private readonly System.Single _durationSeconds;

    private System.Single _elapsed;
    private System.Boolean _hasSwitched;

    #endregion Fields

    #region Construction

    /// <summary>
    /// Initializes a new <see cref="SceneTransition"/>.
    /// </summary>
    /// <param name="nextScene">Target scene name to switch to at transition midpoint.</param>
    /// <param name="style">Overlay transition visual style.</param>
    /// <param name="duration">Total transition duration in seconds (min 0.1s).</param>
    /// <param name="color">Overlay color (default: black).</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="nextScene"/> is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="duration"/> is not positive.</exception>
    public SceneTransition(System.String nextScene, SceneTransitionEffect style = SceneTransitionEffect.Fade, System.Single duration = 1.0f, Color? color = null)
    {
        _nextSceneName = nextScene ?? throw new System.ArgumentNullException(nameof(nextScene));
        _durationSeconds = System.MathF.Max(0.1f, duration);
        _effect = style switch
        {
            SceneTransitionEffect.Fade => new FadeOverlay(color ?? Color.Black),
            SceneTransitionEffect.WipeVertical => new WipeOverlayVertical(color ?? Color.Black),
            SceneTransitionEffect.ZoomIn => new ZoomOverlay(color ?? Color.Black, modeIn: true),
            SceneTransitionEffect.ZoomOut => new ZoomOverlay(color ?? Color.Black, modeIn: false),
            SceneTransitionEffect.WipeHorizontal => new WipeOverlayHorizontal(color ?? Color.Black),
            SceneTransitionEffect.SlideCoverLeft => new SlideCoverOverlay(color ?? Color.Black, fromLeft: true),
            SceneTransitionEffect.SlideCoverRight => new SlideCoverOverlay(color ?? Color.Black, fromLeft: false),
            _ => new FadeOverlay(color ?? Color.Black)
        };

        // Always render on top, persistent through scene change
        this.IsPersistent = true;
        base.SetZIndex(System.Int32.MaxValue);
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Advances the transition state, switches the scene at midpoint, destroys itself on completion.
    /// </summary>
    /// <param name="deltaTime">Elapsed time (seconds) since last frame.</param>
    public override void Update(System.Single deltaTime)
    {
        _elapsed += deltaTime;

        System.Single half = _durationSeconds * 0.5f;
        System.Boolean isClosing = _elapsed <= half;

        System.Single localT = isClosing
            ? (_elapsed / half)
            : ((_elapsed - half) / half);

        localT = System.Math.Clamp(localT, 0f, 1f);

        _effect.Update(localT, isClosing);

        if (!_hasSwitched && _elapsed >= half)
        {
            _hasSwitched = true;
            SceneManager.Instance.RequestSceneChange(_nextSceneName);
        }

        if (_elapsed >= _durationSeconds)
        {
            base.Destroy();
        }
    }

    /// <summary>
    /// Gets the current overlay drawable.
    /// </summary>
    protected override Drawable GetDrawable() => _effect.GetDrawable();

    #endregion Virtual Methods
}