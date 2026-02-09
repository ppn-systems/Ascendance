// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Managers;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.ServerSelection.View;

/// <summary>
/// Manages the background image and dark overlay for the server selection screen.
/// </summary>
public sealed class BackdropView : RenderObject
{
    #region Constants

    private static readonly Color BackgroundOverlay = new(15, 20, 30, 160);

    #endregion Constants

    #region Fields

    private readonly Sprite _background;
    private readonly RectangleShape _darkOverlay;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="BackdropView"/> class.
    /// </summary>
    public BackdropView()
    {
        this.SetZIndex(0);
        // Load and setup background
        Texture bgTexture = AssetManager.Instance.LoadTexture("res/texture/wcp/0");
        _background = new Sprite(bgTexture)
        {
            Position = new Vector2f(0, 0)
        };

        // Scale to fill screen
        System.Single scale = System.Math.Max(
            (System.Single)GraphicsEngine.ScreenSize.X / bgTexture.Size.X,
            (System.Single)GraphicsEngine.ScreenSize.Y / bgTexture.Size.Y
        );
        _background.Scale = new Vector2f(scale, scale);

        // Center background
        System.Single offsetX = (GraphicsEngine.ScreenSize.X - (bgTexture.Size.X * scale)) * 0.5f;
        System.Single offsetY = (GraphicsEngine.ScreenSize.Y - (bgTexture.Size.Y * scale)) * 0.5f;
        _background.Position = new Vector2f(offsetX, offsetY);

        // Dark overlay
        _darkOverlay = new RectangleShape((Vector2f)GraphicsEngine.ScreenSize)
        {
            FillColor = BackgroundOverlay
        };
    }

    #endregion Constructor

    #region Overrides

    /// <summary>
    /// Renders the background and overlay.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);
        target.Draw(_darkOverlay);
    }

    /// <summary>
    /// Returns the background sprite as the main drawable.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => _background;

    #endregion Overrides
}