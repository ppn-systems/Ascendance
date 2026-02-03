// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.UI.Theme;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Very simple loading overlay, draws a dimmed background.
/// Spinner is handled as a separate object.
/// </summary>
public sealed class LoadingOverlay : RenderObject
{
    #region Constants

    private const System.Byte DefaultOverlayAlpha = 160;

    #endregion

    #region Fields

    private readonly Spinner _spinner;
    private readonly RectangleShape _overlayRect;

    #endregion

    #region Constructor

    public LoadingOverlay()
    {
        _overlayRect = new RectangleShape(new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y))
        {
            FillColor = new Color(0, 0, 0, DefaultOverlayAlpha), // Black, alpha 160
            Position = default
        };


        base.SetZIndex(RenderLayer.Overlay.ToZIndex());

        _spinner = new Spinner(new Vector2f(GraphicsEngine.ScreenSize.X / 2f, GraphicsEngine.ScreenSize.Y / 2f));
        _spinner.SetRotationSpeed(180f)
                .SetZIndex(System.Int32.MaxValue - 1); // 180 degrees per second
    }

    #endregion

    #region Public API

    /// <summary>
    /// Sets the overlay background color and alpha (default is dark semi-transparent).
    /// </summary>
    public LoadingOverlay SetOverlayColor(Color color, System.Byte? alpha = null)
    {
        var a = alpha ?? DefaultOverlayAlpha;
        _overlayRect.FillColor = new Color(color.R, color.G, color.B, a);
        return this;
    }

    #endregion Public API

    #region Main Loop

    public override void Update(System.Single deltaTime)
    {
        // If window resized → resize overlay rectangle
        if (_overlayRect.Size.X != GraphicsEngine.ScreenSize.X ||
            _overlayRect.Size.Y != GraphicsEngine.ScreenSize.Y)
        {
            _overlayRect.Size = new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);
        }

        _spinner.Update(deltaTime);
    }

    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_overlayRect);
        _spinner.Draw(target);
    }

    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Overlay uses its own drawing routine.");

    #endregion

    #region Class

    /// <summary>
    /// Procedural animated spinner used as a loading indicator.
    /// Can be shown independently or embedded as part of composite UI.
    /// </summary>
    /// <remarks>
    /// This spinner is designed for efficient rendering by precomputing segment shapes and alpha multipliers
    /// to avoid unnecessary allocations during each frame.
    /// </remarks>
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
        /// Initializes a new instance of the <see cref="Spinner"/> class at a specific center point.
        /// </summary>
        /// <param name="center">The center point for the spinner.</param>
        public Spinner(Vector2f center)
        {
            _center = center;
            this.PRECOMPUTE_SEGMENTS();
            base.SetZIndex(RenderLayer.Spinner.ToZIndex());
        }

        #endregion Constructor

        #region API

        /// <summary>
        /// Sets the alpha (opacity) for the entire spinner.
        /// </summary>
        /// <param name="alpha">The alpha value (0-255).</param>
        /// <returns>The <see cref="Spinner"/> instance, for chaining.</returns>
        public Spinner SetAlpha(System.Byte alpha)
        {
            _alpha = alpha;
            return this;
        }

        /// <summary>
        /// Sets the spinner's rotation speed in degrees per second.
        /// </summary>
        /// <param name="degreesPerSecond">The rotation speed in degrees per second.</param>
        /// <returns>The <see cref="Spinner"/> instance, for chaining.</returns>
        public Spinner SetRotationSpeed(System.Single degreesPerSecond)
        {
            _rotationDegreesPerSecond = degreesPerSecond;
            return this;
        }

        /// <summary>
        /// Updates the center location of the spinner.
        /// </summary>
        /// <param name="newCenter">The new center point for the spinner.</param>
        /// <returns>The <see cref="Spinner"/> instance, for chaining.</returns>
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
                segCircle.FillColor = new Color(Themes.SpinnerForegroundColor.R, Themes.SpinnerForegroundColor.G, Themes.SpinnerForegroundColor.B, finalAlpha);

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

    #endregion Class
}