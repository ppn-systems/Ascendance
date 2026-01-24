// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
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


        base.SetZIndex(System.Int32.MaxValue - 2);

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

    #endregion

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
}