using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Displays an animated loading spinner overlay with a dimmed background, rotating icon, and scale oscillation.
/// Includes fade-in/fade-out transitions, Show/Hide API, and automatically re-centers on resolution changes.
/// </summary>
public sealed class LoadingSpinner : RenderObject
{
    #region Constants

    /// <summary>
    /// The maximum alpha (opacity) for the spinner and overlay.
    /// </summary>
    private const System.Single MaxAlpha = 255f;

    /// <summary>
    /// Conversion constant from degrees to radians.
    /// </summary>
    private const System.Single DegreesToRadians = 0.017453292519943295f;

    /// <summary>
    /// Default overlay alpha value for the dimmed background during fade-in.
    /// </summary>
    private const System.Byte DefaultOverlayAlpha = 160;

    #endregion

    #region Fields

    /// <summary>
    /// The rotation speed of the spinner in degrees per second.
    /// </summary>
    private System.Single _rotationDegreesPerSecond = 150f;

    /// <summary>
    /// The alpha fade speed, as units per second.
    /// </summary>
    private System.Single _fadeAlphaPerSecond = 300f;

    /// <summary>
    /// The baseline scale for the spinner (1.0 = native size).
    /// </summary>
    private System.Single _baseScale = 0.6f;

    /// <summary>
    /// Amplitude of the scale oscillation for the spinner (sinusoidal floating effect).
    /// </summary>
    private System.Single _oscillationAmplitude = 0.02f;

    /// <summary>
    /// Current rotation angle (degrees) of the spinner.
    /// </summary>
    private System.Single _currentAngle = 0f;

    /// <summary>
    /// The current alpha value in float, for smooth transitions.
    /// </summary>
    private System.Single _currentAlphaF = 0f;

    /// <summary>
    /// Whether fade-in is active.
    /// </summary>
    private System.Boolean _isFadingIn = true;

    /// <summary>
    /// Whether fade-out is active.
    /// </summary>
    private System.Boolean _isFadingOut = false;

    /// <summary>
    /// The current alpha value as a byte (applied to overlay and spinner).
    /// </summary>
    private System.Byte _currentAlphaByte = 0;

    /// <summary>
    /// Tracks the last detected screen size to reposition spinner/overlay if changed.
    /// </summary>
    private Vector2u _previousScreenSize = GraphicsEngine.ScreenSize;

    /// <summary>
    /// The dimmed overlay background rectangle.
    /// </summary>
    private readonly RectangleShape _overlayRect;

    /// <summary>
    /// The rotating spinner icon sprite.
    /// </summary>
    private readonly Sprite _spinnerSprite;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets whether the spinner blocks input events to UI elements beneath the overlay.
    /// Default is <c>true</c>.
    /// </summary>
    public System.Boolean IsBlockingInput { get; set; } = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingSpinner"/> class using the provided spinner texture
    /// and the current screen size. Icon is centered and spinner is shown by default with fade-in.
    /// </summary>
    /// <param name="spinnerTexture">The texture for the spinning icon (should be square, centered).</param>
    public LoadingSpinner(Texture spinnerTexture)
    {
        var screenSize = GraphicsEngine.ScreenSize;
        var sizeVec = new Vector2f(screenSize.X, screenSize.Y);

        // Create overlay rectangle covering the screen, initially transparent.
        _overlayRect = new RectangleShape(sizeVec)
        {
            FillColor = new Color(0, 0, 0, 0),
            Position = default
        };

        // Create and center the spinner sprite.
        _spinnerSprite = new Sprite(spinnerTexture)
        {
            Origin = new Vector2f(spinnerTexture.Size.X * 0.5f, spinnerTexture.Size.Y * 0.5f),
            Position = new Vector2f(sizeVec.X * 0.5f, sizeVec.Y * 0.5f),
            Scale = new Vector2f(_baseScale, _baseScale),
            Color = new Color(255, 255, 255, 0)
        };

        Show(); // Show immediately, spinner will fade in
    }

    #endregion

    #region Public API (Fluent)

    /// <summary>
    /// Makes the spinner visible, with a fade-in effect.
    /// </summary>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public new LoadingSpinner Show()
    {
        base.Show();
        _isFadingIn = true;
        _isFadingOut = false;
        return this;
    }

    /// <summary>
    /// Hides the spinner, using a fade-out effect.
    /// </summary>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public new LoadingSpinner Hide()
    {
        base.Hide();
        _isFadingOut = true;
        _isFadingIn = false;
        return this;
    }

    /// <summary>
    /// Updates the spinner icon texture (used for icon theming or atlas swapping).
    /// </summary>
    /// <param name="texture">The new spinner icon <see cref="Texture"/>.</param>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public LoadingSpinner SetSpinnerIcon(Texture texture)
    {
        _spinnerSprite.Texture = texture;
        _spinnerSprite.Origin = new Vector2f(texture.Size.X * 0.5f, texture.Size.Y * 0.5f);
        CENTER_SPINNER();
        return this;
    }

    /// <summary>
    /// Sets the color of the dimmed overlay background. Alpha is always controlled by fade transitions.
    /// </summary>
    /// <param name="color">The RGB color for the overlay (alpha is ignored).</param>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public LoadingSpinner SetOverlayColor(Color color)
    {
        _overlayRect.FillColor = new Color(color.R, color.G, color.B, _currentAlphaByte);
        return this;
    }

    /// <summary>
    /// Sets both the rotation speed (degrees per second) and the alpha fade speed (units per second).
    /// </summary>
    /// <param name="rotationDegreesPerSecond">Spinner rotation speed in degrees per second.</param>
    /// <param name="fadeAlphaPerSecond">Alpha transition speed (0-255 units per second).</param>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public LoadingSpinner SetSpeeds(System.Single rotationDegreesPerSecond, System.Single fadeAlphaPerSecond)
    {
        _rotationDegreesPerSecond = rotationDegreesPerSecond;
        _fadeAlphaPerSecond = fadeAlphaPerSecond;
        return this;
    }

    /// <summary>
    /// Sets the base scale for the spinner icon and the amplitude of its scale oscillation.
    /// </summary>
    /// <param name="baseScale">Baseline scale (1.0 = native size).</param>
    /// <param name="oscillationAmplitude">Scale oscillation amplitude (default 0.02).</param>
    /// <returns>This spinner instance for fluent chaining.</returns>
    public LoadingSpinner SetScale(System.Single baseScale, System.Single oscillationAmplitude = 0.02f)
    {
        _baseScale = baseScale;
        _oscillationAmplitude = oscillationAmplitude;
        return this;
    }

    #endregion

    #region Main Loop

    /// <summary>
    /// Performs per-frame updates: handles fading, repositioning on screen size change,
    /// rotating the spinner, and scale oscillation.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame (in seconds).</param>
    public override void Update(System.Single deltaTime)
    {
        if (!IsVisible && !_isFadingOut)
        {
            return;
        }

        // Recenter overlay and spinner if the screen size changed.
        if (_previousScreenSize != GraphicsEngine.ScreenSize)
        {
            _previousScreenSize = GraphicsEngine.ScreenSize;
            RESIZE_OVERLAY();
            CENTER_SPINNER();
        }

        UPDATE_ALPHA_TRANSITION(deltaTime);

        // Rotate spinner
        _currentAngle += deltaTime * _rotationDegreesPerSecond;
        if (_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }

        _spinnerSprite.Rotation = _currentAngle;

        // Oscillate scale using a sine wave for a floating effect
        System.Single scale = _baseScale + (System.MathF.Sin(_currentAngle * DegreesToRadians) * _oscillationAmplitude);
        _spinnerSprite.Scale = new Vector2f(scale, scale);
    }

    /// <summary>
    /// Renders the overlay background and spinner icon to the specified target.
    /// </summary>
    /// <param name="target">The SFML <see cref="RenderTarget"/> to render to.</param>
    public void Render(RenderTarget target)
    {
        if (!IsVisible && !_isFadingOut)
        {
            return;
        }

        target.Draw(_overlayRect);
        target.Draw(_spinnerSprite);
    }

    /// <summary>
    /// Not supported for <see cref="LoadingSpinner"/>—use <see cref="Render(RenderTarget)"/> instead.
    /// </summary>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="System.NotSupportedException">Always thrown.</exception>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Private Helpers

    /// <summary>
    /// Handles per-frame alpha transitions for fade-in and fade-out.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    private void UPDATE_ALPHA_TRANSITION(System.Single deltaTime)
    {
        // Don't update if fully hidden and not transitioning
        if (!_isFadingIn && !_isFadingOut && !IsVisible)
        {
            return;
        }

        if (_isFadingIn)
        {
            _currentAlphaF += deltaTime * _fadeAlphaPerSecond;
            if (_currentAlphaF >= MaxAlpha)
            {
                _currentAlphaF = MaxAlpha;
                _isFadingIn = false;
                base.Show();
            }
        }
        else if (_isFadingOut)
        {
            _currentAlphaF -= deltaTime * _fadeAlphaPerSecond;
            if (_currentAlphaF <= 0f)
            {
                _currentAlphaF = 0f;
                _isFadingOut = false;
                base.Hide();
            }
        }

        System.Byte newAlpha = (System.Byte)System.Math.Clamp(_currentAlphaF, 0f, MaxAlpha);
        if (newAlpha == _currentAlphaByte)
        {
            return;
        }

        _currentAlphaByte = newAlpha;

        // Apply alpha to overlay
        var baseColor = _overlayRect.FillColor;
        _overlayRect.FillColor = new Color(baseColor.R, baseColor.G, baseColor.B, _currentAlphaByte);

        // Apply alpha to spinner icon
        var spinnerColor = _spinnerSprite.Color;
        _spinnerSprite.Color = new Color(spinnerColor.R, spinnerColor.G, spinnerColor.B, _currentAlphaByte);
    }

    /// <summary>
    /// Adjusts the overlay rectangle size to cover the current screen size.
    /// </summary>
    private void RESIZE_OVERLAY()
    {
        var screenSize = GraphicsEngine.ScreenSize;
        _overlayRect.Size = new Vector2f(screenSize.X, screenSize.Y);
    }

    /// <summary>
    /// Centers the spinner icon in the current screen.
    /// Sets base overlay alpha if necessary.
    /// </summary>
    private void CENTER_SPINNER()
    {
        var screenSize = GraphicsEngine.ScreenSize;
        _spinnerSprite.Position = new Vector2f(screenSize.X * 0.5f, screenSize.Y * 0.5f);

        // If overlay does not yet have alpha, assign default for fade-in
        if (_overlayRect.FillColor.A == 0)
        {
            _overlayRect.FillColor = new Color(0, 0, 0, System.Math.Min(DefaultOverlayAlpha, _currentAlphaByte));
        }
    }

    #endregion
}