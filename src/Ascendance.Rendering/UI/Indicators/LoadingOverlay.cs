// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Displays an animated loading spinner overlay with a dimmed background, rotating bar segments,
/// and scale oscillation, all without requiring a texture.
/// Includes fade-in/fade-out transitions, Show/Hide API, and auto-centering on resolution changes.
/// </summary>
public sealed class LoadingOverlay : RenderObject, IUpdatable
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

    /// <summary>
    /// Number of bar segments composing the spinner.
    /// </summary>
    private const System.Int32 SegmentCount = 12;

    /// <summary>
    /// Spinner bar radius.
    /// </summary>
    private const System.Single SpinnerRadius = 32f;

    /// <summary>
    /// Thickness of each spinner bar segment.
    /// </summary>
    private const System.Single SegmentThickness = 7f;

    #endregion Constants

    #region Fields

    private System.Single _baseScale = 1.0f;
    private System.Single _fadeAlphaPerSecond = 300f;
    private System.Single _oscillationAmplitude = 0.06f;
    private System.Single _rotationDegreesPerSecond = 150f;

    private System.Single _currentAngle = 0f;
    private System.Single _currentAlphaF = 0f;
    private System.Byte _currentAlphaByte = 0;
    private System.Boolean _isFadingIn = true;
    private System.Boolean _isFadingOut = false;
    private Vector2u _previousScreenSize = GraphicsEngine.ScreenSize;

    /// <summary>
    /// The dimmed overlay background rectangle.
    /// </summary>
    private readonly RectangleShape _overlayRect;

    /// <summary>
    /// Current spinner center (auto-updated per resize).
    /// </summary>
    private Vector2f _spinnerCenter;

    /// <summary>
    /// Base color for the spinner bars.
    /// </summary>
    private Color _spinnerColor = new(95, 161, 255); // Customize as needed

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets whether the spinner blocks input events to UI elements beneath the overlay.
    /// Default is <c>true</c>.
    /// </summary>
    public System.Boolean IsBlockingInput { get; set; } = true;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingOverlay"/> class.
    /// Spinner uses procedural geometry—no textures needed!
    /// </summary>
    public LoadingOverlay()
    {
        var screenSize = GraphicsEngine.ScreenSize;
        var sizeVec = new Vector2f(screenSize.X, screenSize.Y);

        _overlayRect = new RectangleShape(sizeVec)
        {
            FillColor = new Color(0, 0, 0, 0),
            Position = default
        };

        UPDATE_SPINNER_CENTER();
        Show();
    }

    #endregion Constructor

    #region Public API (Fluent)

    public new LoadingOverlay Show()
    {
        base.Show();
        _isFadingIn = true;
        _isFadingOut = false;
        return this;
    }
    public new LoadingOverlay Hide()
    {
        base.Hide();
        _isFadingOut = true;
        _isFadingIn = false;
        return this;
    }
    /// <summary>
    /// Sets the color of the dimmed overlay background. Alpha is always controlled by fade transitions.
    /// </summary>
    public LoadingOverlay SetOverlayColor(Color color)
    {
        _overlayRect.FillColor = new Color(color.R, color.G, color.B, _currentAlphaByte);
        return this;
    }
    /// <summary>
    /// Sets the color for the spinner bar segments.
    /// </summary>
    public LoadingOverlay SetSpinnerColor(Color color)
    {
        _spinnerColor = new Color(color.R, color.G, color.B, _currentAlphaByte);
        return this;
    }
    /// <summary>
    /// Sets both the rotation speed (degrees per second) and the alpha fade speed (units per second).
    /// </summary>
    public LoadingOverlay SetSpeeds(System.Single rotationDegreesPerSecond, System.Single fadeAlphaPerSecond)
    {
        _rotationDegreesPerSecond = rotationDegreesPerSecond;
        _fadeAlphaPerSecond = fadeAlphaPerSecond;
        return this;
    }
    /// <summary>
    /// Sets the base scale for the spinner and the amplitude of its scale oscillation.
    /// </summary>
    public LoadingOverlay SetScale(System.Single baseScale, System.Single oscillationAmplitude = 0.06f)
    {
        _baseScale = baseScale;
        _oscillationAmplitude = oscillationAmplitude;
        return this;
    }

    #endregion

    #region Main Loop

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
            UPDATE_SPINNER_CENTER();
        }

        UPDATE_ALPHA_TRANSITION(deltaTime);

        // Rotate spinner
        _currentAngle += deltaTime * _rotationDegreesPerSecond;
        if (_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }
    }

    /// <summary>
    /// Renders the overlay background and procedural spinner to the specified target.
    /// </summary>
    /// <param name="target">The SFML <see cref="RenderTarget"/> to render to.</param>
    public void Render(RenderTarget target)
    {
        if (!IsVisible && !_isFadingOut)
        {
            return;
        }

        target.Draw(_overlayRect);
        DRAW_SPINNER(target);
    }

    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion Public API (Fluent)

    #region Private Helpers

    /// <summary>
    /// Fade transitions for spinner and overlay.
    /// </summary>
    private void UPDATE_ALPHA_TRANSITION(System.Single deltaTime)
    {
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

        // Update spinner color's alpha as well (segment draws will pull from _currentAlphaByte)
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
    /// Update spinner center based on screen size.
    /// </summary>
    private void UPDATE_SPINNER_CENTER()
    {
        var screenSize = GraphicsEngine.ScreenSize;
        _spinnerCenter = new Vector2f(screenSize.X * 0.5f, screenSize.Y * 0.5f);
        if (_overlayRect.FillColor.A == 0)
        {
            _overlayRect.FillColor = new Color(0, 0, 0, System.Math.Min(DefaultOverlayAlpha, _currentAlphaByte));
        }
    }

    /// <summary>
    /// Draw the procedural spinner using rotated bar segments.
    /// </summary>
    private void DRAW_SPINNER(RenderTarget target)
    {
        const System.Single anglePerSegment = 360f / SegmentCount;

        // Base scale plus oscillation ("breathing" effect)
        System.Single scale = _baseScale + (System.MathF.Sin(_currentAngle * DegreesToRadians) * _oscillationAmplitude);

        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            System.Single progress = (System.Single)i / SegmentCount;
            System.Byte segmentAlpha = (System.Byte)(_currentAlphaByte * (0.2f + (0.8f * progress))); // trail effect

            System.Single segAngle = _currentAngle + (i * anglePerSegment);
            System.Single angleRad = segAngle * DegreesToRadians;

            System.Single radius = SpinnerRadius * scale;
            System.Single x = _spinnerCenter.X + (System.MathF.Cos(angleRad) * radius);
            System.Single y = _spinnerCenter.Y + (System.MathF.Sin(angleRad) * radius);

            // Draw segment as a filled small circle ("dot style" spinner)
            var segCircle = new CircleShape(SegmentThickness * scale / 2f)
            {
                Origin = new Vector2f(SegmentThickness * scale / 2f, SegmentThickness * scale / 2f),
                Position = new Vector2f(x, y),
                FillColor = new Color(_spinnerColor.R, _spinnerColor.G, _spinnerColor.B, segmentAlpha)
            };
            target.Draw(segCircle);
        }
    }

    #endregion Private Helpers
}