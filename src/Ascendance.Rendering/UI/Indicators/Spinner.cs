// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.UI.Theme;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Procedural animated spinner used as a loading indicator.
/// Can be shown independently or embedded as part of composite UI.
/// Hiệu năng tối ưu: giảm new object mỗi frame, precompute các giá trị.
/// </summary>
public sealed class Spinner : RenderObject, IUpdatable
{
    #region Constants

    private const System.Int32 SegmentCount = 12;
    private const System.Single SpinnerRadius = 32f;
    private const System.Single SegmentThickness = 7f;
    private const System.Single DegreesToRadians = 0.017453292519943295f;

    #endregion Constants

    #region Fields

    private Vector2f _center;
    private System.Byte _alpha = 255;
    private System.Single _currentAngle = 0f;
    private System.Single _rotationDegreesPerSecond = 150f;

    // Precomputed values to avoid re-allocating every Draw
    private readonly CircleShape[] _segmentShapes = new CircleShape[SegmentCount];
    private readonly System.Single[] _segmentOffsets = new System.Single[SegmentCount];
    private readonly System.Byte[] _segmentAlphaMultipliers = new System.Byte[SegmentCount];

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Constructs a new spinner instance at a given center.
    /// </summary>
    /// <param name="center">Center point for the spinner.</param>
    public Spinner(Vector2f center)
    {
        _center = center;
        this.PRECOMPUTE_SEGMENTS();
    }

    #endregion Constructor

    #region API

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

    /// <inheritdoc />
    public override void Draw(RenderTarget target)
    {

        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            System.Single segAngle = _currentAngle + _segmentOffsets[i];
            System.Single angleRad = segAngle * DegreesToRadians;

            System.Single x = _center.X + (System.MathF.Cos(angleRad) * SpinnerRadius);
            System.Single y = _center.Y + (System.MathF.Sin(angleRad) * SpinnerRadius);

            CircleShape segCircle = _segmentShapes[i];

            segCircle.Radius = SegmentThickness / 2f;
            segCircle.Origin = new Vector2f(segCircle.Radius, segCircle.Radius);
            segCircle.Position = new Vector2f(x, y);

            System.Byte finalAlpha = (System.Byte)(_alpha * _segmentAlphaMultipliers[i] / 255);
            segCircle.FillColor = new Color(Themes.SpinnerColor.R, Themes.SpinnerColor.G, Themes.SpinnerColor.B, finalAlpha);

            target.Draw(segCircle);
        }
    }

    /// <inheritdoc />
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Spinner uses procedural geometry. Call Render() directly.");

    #endregion Main Loop

    #region Private Methods

    /// <summary>
    /// Precomputes static values for segment angle and multipliers to optimize drawing.
    /// </summary>
    private void PRECOMPUTE_SEGMENTS()
    {
        const System.Single anglePerSegment = 360f / SegmentCount;

        for (System.Int32 i = 0; i < SegmentCount; i++)
        {
            // Offset angle for this segment (degrees)
            _segmentOffsets[i] = i * anglePerSegment;

            // trailing tail alpha effect: 0.2f + 0.8f * progress => multiply by 255 (max alpha)
            System.Single progress = (System.Single)i / SegmentCount;
            System.Single alphaMultiplier = 0.2f + (0.8f * progress);
            _segmentAlphaMultipliers[i] = (System.Byte)(alphaMultiplier * 255);

            // Init CircleShape ONCE, just set position/color each draw
            _segmentShapes[i] = new CircleShape(SegmentThickness / 2f)
            {
                Origin = new Vector2f(SegmentThickness / 2f, SegmentThickness / 2f)
            };
        }
    }

    #endregion Private Methods
}