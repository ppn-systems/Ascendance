// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Graphics.Transitions.Effects;
using Ascendance.Rendering.Scenes;
using SFML.Graphics;

namespace Ascendance.Rendering.Graphics.Transitions;

/// <summary>
/// Runs a two-phase scene transition via a full-screen overlay: <b>closing</b> (cover) → <b>switch scene</b> → <b>opening</b> (reveal).
/// (VN) Hiệu ứng chuyển cảnh 2 pha (đóng → đổi cảnh → mở) thông qua một lớp overlay hiệu ứng.
/// </summary>
/// <remarks>
/// Instance này tồn tại xuyên cảnh và tự hủy khi hoàn tất.
/// Dùng hiệu ứng vẽ được chọn qua <see cref="ITransitionDrawable"/> strategy.
/// </remarks>
public sealed class SceneTransition : RenderObject, IRenderUpdatable
{
    #region Fields (configuration)

    private readonly System.Single _durationSeconds;
    private readonly System.String _nextSceneName;
    private readonly ITransitionDrawable _effect;

    #endregion

    #region Runtime state

    private System.Single _elapsed;
    private System.Boolean _hasSwitched;

    #endregion

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
    public SceneTransition(
        System.String nextScene,
        SceneTransitionEffect style = SceneTransitionEffect.Fade,
        System.Single duration = 1.0f,
        Color? color = null)
    {
        _nextSceneName = nextScene ?? throw new System.ArgumentNullException(nameof(nextScene));
        _durationSeconds = System.MathF.Max(0.1f, duration);
        var overlay = color ?? Color.Black;

        _effect = style switch
        {
            SceneTransitionEffect.Fade => new FadeOverlay(overlay),
            SceneTransitionEffect.WipeHorizontal => new WipeOverlayHorizontal(overlay),
            SceneTransitionEffect.WipeVertical => new WipeOverlayVertical(overlay),
            SceneTransitionEffect.SlideCoverLeft => new SlideCoverOverlay(overlay, fromLeft: true),
            SceneTransitionEffect.SlideCoverRight => new SlideCoverOverlay(overlay, fromLeft: false),
            SceneTransitionEffect.ZoomIn => new ZoomOverlay(overlay, modeIn: true),
            SceneTransitionEffect.ZoomOut => new ZoomOverlay(overlay, modeIn: false),
            _ => new FadeOverlay(overlay)
        };

        // Always render on top, persistent through scene change
        SetZIndex(System.Int32.MaxValue);
        PersistOnSceneChange = true;
    }

    #endregion

    #region Engine hooks

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
            SceneManager.ChangeScene(_nextSceneName);
        }

        if (_elapsed >= _durationSeconds)
        {
            Destroy();
        }
    }

    /// <summary>
    /// Gets the current overlay drawable.
    /// </summary>
    protected override Drawable GetDrawable() => _effect.GetDrawable();

    #endregion
}