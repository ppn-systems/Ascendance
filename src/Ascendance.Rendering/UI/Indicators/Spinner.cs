// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Procedural animated spinner used as a loading indicator.
/// Can be shown independently or embedded as part of composite UI.
/// </summary>
public sealed class Spinner : RenderObject, IUpdatable
{
    #region Constants

    /// <summary>
    /// Number of bar segments making up the spinner.
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
    /// <summary>
    /// Conversion constant from degrees to radians.
    /// </summary>
    private const System.Single DegreesToRadians = 0.017453292519943295f;

    #endregion Constants

    #region Fields

    private Vector2f _center;
    private System.Byte _alpha = 255;
    private System.Single _baseScale = 1.0f;
    private System.Single _currentAngle = 0f;
    private Color _spinnerColor = new(255, 255, 255);
    private System.Single _oscillationAmplitude = 0.06f;
    private System.Single _rotationDegreesPerSecond = 150f;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Constructs a new spinner instance at a given center.
    /// </summary>
    /// <param name="center">Center point for the spinner.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Spinner(Vector2f center) => _center = center;

    #endregion Constructor

    #region API

    /// <summary>
    /// Sets the color for the spinner segments.
    /// </summary>
    /// <param name="color">Segment color (alpha channel is managed internally).</param>
    public Spinner SetSpinnerColor(Color color)
    {
        _spinnerColor = new Color(color.R, color.G, color.B, _alpha);
        return this;
    }

    /// <summary>
    /// Sets the alpha (opacity) for the whole spinner.
    /// </summary>
    /// <param name="alpha">Alpha value (0-255).</param>
    public Spinner SetAlpha(System.Byte alpha)
    {
        _alpha = alpha;
        return this;
    }

    /// <summary>
    /// Sets the spinner scale and its oscillation amplitude ("breathing" effect).
    /// </summary>
    /// <param name="scale">Base scale of the spinner.</param>
    /// <param name="oscillation">Oscillation amplitude (default 0.06f).</param>
    public Spinner SetBaseScale(System.Single scale, System.Single oscillation = 0.06f)
    {
        _baseScale = scale;
        _oscillationAmplitude = oscillation;
        return this;
    }

    /// <summary>
    /// Sets the spinner's rotation speed (degrees per second).
    /// </summary>
    /// <param name="degreesPerSecond">Rotation speed.</param>
    public Spinner SetRotationSpeed(System.Single degreesPerSecond)
    {
        _rotationDegreesPerSecond = degreesPerSecond;
        return this;
    }

    /// <summary>
    /// Updates the center location of the spinner.
    /// </summary>
    /// <param name="newCenter">New center point.</param>
    public Spinner SetCenter(Vector2f newCenter)
    {
        _center = newCenter;
        return this;
    }

    #endregion API

    #region Main Loop

    /// <inheritdoc />
    public override void Update(System.Single deltaTime)
    {
        _currentAngle += deltaTime * _rotationDegreesPerSecond;
        if (_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }
    }

    /// <summary>
    /// Renders the spinner into the specified render target.
    /// </summary>
    /// <param name="target">Target for rendering (usually the current window or view).</param>
    public override void Draw(RenderTarget target)
    {
        const System.Single anglePerSegment = 360f / SegmentCount;

        System.Single scale = _baseScale + (System.MathF.Sin(_currentAngle * DegreesToRadians) * _oscillationAmplitude);

        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            System.Single progress = (System.Single)i / SegmentCount;
            System.Byte segmentAlpha = (System.Byte)(_alpha * (0.2f + (0.8f * progress))); // trailing "tail" effect

            System.Single segAngle = _currentAngle + (i * anglePerSegment);
            System.Single angleRad = segAngle * DegreesToRadians;

            System.Single radius = SpinnerRadius * scale;
            System.Single x = _center.X + (System.MathF.Cos(angleRad) * radius);
            System.Single y = _center.Y + (System.MathF.Sin(angleRad) * radius);

            // Draw segment as a filled circle (dot style)
            CircleShape segCircle = new(SegmentThickness * scale / 2f)
            {
                Position = new Vector2f(x, y),
                Origin = new Vector2f(SegmentThickness * scale / 2f, SegmentThickness * scale / 2f),
                FillColor = new Color(_spinnerColor.R, _spinnerColor.G, _spinnerColor.B, segmentAlpha)
            };
            target.Draw(segCircle);
        }
    }

    /// <inheritdoc />
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Spinner uses procedural geometry. Call Render() directly.");

    #endregion Main Loop
}