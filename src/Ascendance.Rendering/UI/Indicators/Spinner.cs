// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Procedural animated spinner used as a loading indicator. Optimized for low allocations.
/// Reuses segment shapes for better performance.
/// </summary>
public sealed class Spinner : RenderObject, IUpdatable
{
    #region Constants

    /// <summary>
    /// Number of bar segments composing the spinner.
    /// </summary>
    private const System.Int32 SegmentCount = 12;

    /// <summary>
    /// Spinner radius (distance from center to segment centers).
    /// </summary>
    private const System.Single SpinnerRadius = 32f;

    /// <summary>
    /// Thickness of each spinner bar segment.
    /// </summary>
    private const System.Single SegmentThickness = 7f;

    /// <summary>
    /// Conversion factor from degrees to radians.
    /// </summary>
    private const System.Single DegreesToRadians = 0.017453292519943295f;

    #endregion

    #region Fields

    /// <summary>
    /// The center point of the spinner on the screen.
    /// </summary>
    private Vector2f _center;

    /// <summary>
    /// The base color for the spinner segments.
    /// </summary>
    private Color _spinnerColor = new(255, 255, 255);

    /// <summary>
    /// The base (mean) scale for the spinner.
    /// </summary>
    private System.Single _baseScale = 1.0f;

    /// <summary>
    /// Amplitude of scale oscillation ("breathing" effect).
    /// </summary>
    private System.Single _oscillationAmplitude = 0.06f;

    /// <summary>
    /// The spinner's current total rotation angle, in degrees.
    /// </summary>
    private System.Single _currentAngle = 0f;

    /// <summary>
    /// Spinner rotation speed in degrees per second.
    /// </summary>
    private System.Single _rotationDegreesPerSecond = 150f;

    /// <summary>
    /// Opacity (alpha) of the spinner, from 0 (transparent) to 255 (opaque).
    /// </summary>
    private System.Byte _alpha = 255;

    /// <summary>
    /// Cached pool of CircleShape objects, one per segment, reused every frame.
    /// </summary>
    private readonly CircleShape[] _segmentShapes;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="Spinner"/> at a specified center position.
    /// </summary>
    /// <param name="center">The coordinate to center the spinner on screen.</param>
    public Spinner(Vector2f center)
    {
        _center = center;
        _segmentShapes = new CircleShape[SegmentCount];
        System.Single r = SegmentThickness * _baseScale / 2f;
        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            var seg = new CircleShape(r)
            {
                Origin = new Vector2f(r, r),
                FillColor = _spinnerColor
            };
            _segmentShapes[i] = seg;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Sets the color for the spinner segments.
    /// </summary>
    /// <param name="color">The RGB segment color. Alpha is managed internally via <see cref="SetAlpha"/>.</param>
    /// <returns>This spinner for fluent chaining.</returns>
    public Spinner SetSpinnerColor(Color color)
    {
        _spinnerColor = new Color(color.R, color.G, color.B, _alpha);
        return this;
    }

    /// <summary>
    /// Sets the overall spinner opacity.
    /// </summary>
    /// <param name="alpha">Alpha value (0 transparent, 255 opaque).</param>
    /// <returns>This spinner for fluent chaining.</returns>
    public Spinner SetAlpha(System.Byte alpha)
    {
        _alpha = alpha;
        return this;
    }

    /// <summary>
    /// Sets the base scale and oscillation amplitude.
    /// </summary>
    /// <param name="scale">Base spinner scale (1.0 = normal).</param>
    /// <param name="oscillation">Oscillation amplitude (default 0.06f).</param>
    /// <returns>This spinner for fluent chaining.</returns>
    public Spinner SetBaseScale(System.Single scale, System.Single oscillation = 0.06f)
    {
        _baseScale = scale;
        _oscillationAmplitude = oscillation;
        return this;
    }

    /// <summary>
    /// Sets the rotation speed (degrees per second).
    /// </summary>
    /// <param name="degreesPerSecond">Spinner rotation speed.</param>
    /// <returns>This spinner for fluent chaining.</returns>
    public Spinner SetRotationSpeed(System.Single degreesPerSecond)
    {
        _rotationDegreesPerSecond = degreesPerSecond;
        return this;
    }

    /// <summary>
    /// Updates the center position of the spinner to a new value.
    /// </summary>
    /// <param name="newCenter">The new center coordinates.</param>
    /// <returns>This spinner for fluent chaining.</returns>
    public Spinner SetCenter(Vector2f newCenter)
    {
        _center = newCenter;
        return this;
    }

    #endregion

    #region Main Loop

    /// <summary>
    /// Advances the spinner's animation by a time step (in seconds).
    /// </summary>
    /// <param name="deltaTime">Elapsed time since last update, in seconds.</param>
    public override void Update(System.Single deltaTime)
    {
        _currentAngle += deltaTime * _rotationDegreesPerSecond;
        if (_currentAngle >= 360f)
        {
            _currentAngle -= 360f;
        }
    }

    /// <summary>
    /// Draws the spinner segments efficiently without excessive allocations.
    /// Segment shapes are reused and only position/size/color are updated.
    /// </summary>
    /// <param name="target">Render target for drawing (e.g. the game window).</param>
    public override void Draw(RenderTarget target)
    {
        const System.Single anglePerSegment = 360f / SegmentCount;
        System.Single scale = _baseScale + (System.MathF.Sin(_currentAngle * DegreesToRadians) * _oscillationAmplitude);
        System.Single segRadius = SpinnerRadius * scale;
        System.Single segShapeRadius = SegmentThickness * scale / 2f;

        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            System.Single progress = (System.Single)i / SegmentCount;
            System.Byte segmentAlpha = (System.Byte)(_alpha * (0.2f + (0.8f * progress))); // trail effect

            System.Single segAngle = _currentAngle + (i * anglePerSegment);
            System.Single angleRad = segAngle * DegreesToRadians;

            System.Single x = _center.X + (System.MathF.Cos(angleRad) * segRadius);
            System.Single y = _center.Y + (System.MathF.Sin(angleRad) * segRadius);

            CircleShape segCircle = _segmentShapes[i];

            // Only adjust size if changed (rare, for dynamic scaling)
            if (segCircle.Radius != segShapeRadius)
            {
                segCircle.Radius = segShapeRadius;
                segCircle.Origin = new Vector2f(segShapeRadius, segShapeRadius);
            }
            segCircle.Position = new Vector2f(x, y);
            segCircle.FillColor = new Color(_spinnerColor.R, _spinnerColor.G, _spinnerColor.B, segmentAlpha);

            target.Draw(segCircle);
        }
    }

    /// <summary>
    /// Returns a dummy <see cref="Drawable"/> to maintain compatibility with render pipeline.
    /// Actual spinner rendering is handled by <see cref="Draw(RenderTarget)"/>.
    /// </summary>
    /// <returns>An invisible rectangle as a dummy drawable object.</returns>
    protected override Drawable GetDrawable() =>
        new RectangleShape(new Vector2f(1, 1)) { FillColor = new Color(0, 0, 0, 0) };

    #endregion
}