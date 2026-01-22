// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Displays an animated loading spinner overlay with a dimmed background,
/// fade-in/fade-out transitions, and auto-centering on resolution changes.
/// </summary>
public sealed class LoadingOverlay : RenderObject, IUpdatable
{
    #region Constants

    private const System.Single MaxAlpha = 255f;
    private const System.Byte DefaultOverlayAlpha = 160;

    #endregion Constants

    #region Fields

    private Vector2f _spinnerCenter;
    private System.Byte _currentAlphaByte = 0;
    private System.Single _currentAlphaF = 0f;
    private System.Single _fadeAlphaPerSecond = 300f;
    private Vector2u _previousScreenSize = GraphicsEngine.ScreenSize;

    private readonly Spinner _spinner;
    private readonly RectangleShape _overlayRect;

    // The target alpha state, used to determine if overlay is fading in or out.
    private System.Boolean _shouldBeVisible = true;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets whether the overlay blocks input events to UI elements beneath.
    /// </summary>
    public System.Boolean IsBlockingInput { get; set; } = true;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingOverlay"/> class.
    /// </summary>
    public LoadingOverlay()
    {
        _overlayRect = new RectangleShape(new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y))
        {
            FillColor = new Color(0, 0, 0, 0),
            Position = default
        };

        UPDATE_SPINNER_CENTER();
        _spinner = new Spinner(_spinnerCenter);

        this.Show();
        this.SetZIndex(System.Int32.MaxValue - 2);
        _spinner.SetZIndex(System.Int32.MaxValue - 1);
    }

    #endregion Constructor

    #region Public API (Fluent)

    /// <summary>
    /// Shows the overlay (with fade-in transition).
    /// </summary>
    public new LoadingOverlay Show()
    {
        base.Show();
        _spinner.Show();

        _shouldBeVisible = true; // Mark as should be faded in

        return this;
    }

    /// <summary>
    /// Hides the overlay (with fade-out transition).
    /// </summary>
    public new LoadingOverlay Hide()
    {
        base.Hide();
        _spinner.Hide();

        _shouldBeVisible = false; // Mark as should be faded out

        return this;
    }

    /// <summary>
    /// Sets the overlay background color (alpha managed internally).
    /// </summary>
    public LoadingOverlay SetOverlayColor(Color color)
    {
        _overlayRect.FillColor = new Color(color.R, color.G, color.B, _currentAlphaByte);
        return this;
    }

    /// <summary>
    /// Sets the spinner segment color.
    /// </summary>
    public LoadingOverlay SetSpinnerColor(Color color)
    {
        _spinner.SetSpinnerColor(color);
        return this;
    }

    /// <summary>
    /// Sets the spinner speed and fade transition speed.
    /// </summary>
    public LoadingOverlay SetSpeeds(System.Single rotationDegreesPerSecond, System.Single fadeAlphaPerSecond)
    {
        _spinner.SetRotationSpeed(rotationDegreesPerSecond);
        _fadeAlphaPerSecond = fadeAlphaPerSecond;
        return this;
    }

    /// <summary>
    /// Sets the spinner's scale and oscillation amplitude.
    /// </summary>
    public LoadingOverlay SetScale(System.Single baseScale, System.Single oscillationAmplitude = 0.06f)
    {
        _spinner.SetBaseScale(baseScale, oscillationAmplitude);
        return this;
    }

    #endregion Public API (Fluent)

    #region Main Loop

    /// <inheritdoc />
    public override void Update(System.Single deltaTime)
    {
        // Overlay should be updated if it's visible, or alpha is not fully faded
        if (!_shouldBeVisible && _currentAlphaF <= 0f)
        {
            return;
        }

        // Recenter overlay and spinner if window resized
        if (_previousScreenSize != GraphicsEngine.ScreenSize)
        {
            _previousScreenSize = GraphicsEngine.ScreenSize;
            RESIZE_OVERLAY();
            UPDATE_SPINNER_CENTER();
            _spinner.SetCenter(_spinnerCenter);
        }

        UPDATE_ALPHA_TRANSITION(deltaTime);
        _spinner.SetAlpha(_currentAlphaByte);

        _spinner.Update(deltaTime);
    }

    /// <summary>
    /// Renders the overlay and spinner.
    /// </summary>
    /// <param name="target">The render target (screen/window).</param>
    public override void Draw(RenderTarget target)
    {
        // Draw overlay if it's visible or alpha > 0
        if (!_shouldBeVisible && _currentAlphaF <= 0f)
        {
            return;
        }

        target.Draw(_overlayRect);
        _spinner.Draw(target);
    }

    /// <inheritdoc />
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion Main Loop

    #region Private Helpers

    /// <summary>
    /// Handles fade-in/out transitions for both overlay and spinner alpha.
    /// </summary>
    private void UPDATE_ALPHA_TRANSITION(System.Single deltaTime)
    {
        // Determine target alpha
        System.Single targetAlpha = _shouldBeVisible ? MaxAlpha : 0f;

        if (_currentAlphaF == targetAlpha)
        {
            return;
        }

        // Move towards target alpha
        System.Single alphaDelta = _fadeAlphaPerSecond * deltaTime;
        if (_shouldBeVisible)
        {
            _currentAlphaF = System.Math.Min(_currentAlphaF + alphaDelta, MaxAlpha);

            if (_currentAlphaF == MaxAlpha)
            {
                base.Show();
            }
        }
        else
        {
            _currentAlphaF = System.Math.Max(_currentAlphaF - alphaDelta, 0f);

            if (_currentAlphaF == 0f)
            {
                base.Hide();
            }
        }

        System.Byte newAlpha = (System.Byte)System.Math.Clamp(_currentAlphaF, 0f, MaxAlpha);
        if (newAlpha == _currentAlphaByte)
        {
            return;
        }

        _currentAlphaByte = newAlpha;

        // Update overlay rectangle's alpha
        Color baseColor = _overlayRect.FillColor;
        _overlayRect.FillColor = new Color(baseColor.R, baseColor.G, baseColor.B, _currentAlphaByte);
    }

    /// <summary>
    /// Adjusts overlay rectangle to cover the window.
    /// </summary>
    private void RESIZE_OVERLAY() =>
        _overlayRect.Size = new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);

    /// <summary>
    /// Updates center location for the spinner.
    /// </summary>
    private void UPDATE_SPINNER_CENTER()
    {
        _spinnerCenter = new Vector2f(GraphicsEngine.ScreenSize.X * 0.5f, GraphicsEngine.ScreenSize.Y * 0.5f);

        if (_overlayRect.FillColor.A == 0)
        {
            _overlayRect.FillColor = new Color(0, 0, 0, System.Math.Min(DefaultOverlayAlpha, _currentAlphaByte));
        }
    }

    #endregion Private Helpers
}