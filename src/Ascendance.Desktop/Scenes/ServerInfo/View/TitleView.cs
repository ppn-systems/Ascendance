// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.ServerInfo.View;

/// <summary>
/// Represents the title panel at the top of the server selection screen.
/// </summary>
internal sealed class TitleView : RenderObject
{
    #region Constants

    private const System.Single TitleWidth = 280f;
    private const System.Single TitleHeight = 65f;

    private static readonly Thickness PanelBorder = new(32);
    private static readonly Color TitleTextColor = new(45, 30, 15);
    private static readonly Color TitleTextOutline = new(230, 200, 140);
    private static readonly Color TitleBgColor = new(200, 160, 100, 250);

    #endregion Constants

    #region Fields

    private readonly Text _titleText;
    private readonly NineSlicePanel _titlePanel;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleView"/> class.
    /// </summary>
    public TitleView()
    {
        this.SetZIndex(1);
        Font font = EmbeddedAssets.JetBrainsMono.ToFont();
        Texture panelTexture = EmbeddedAssets.SquareOutline.ToTexture();
        Vector2f center = new(GraphicsEngine.ScreenSize.X * 0.5f, GraphicsEngine.ScreenSize.Y * 0.1f);

        Vector2f titlePos = new(center.X - (TitleWidth * 0.5f), center.Y);

        _titlePanel = new NineSlicePanel(panelTexture, PanelBorder, default)
            .SetPosition(titlePos)
            .SetSize(new Vector2f(TitleWidth, TitleHeight));
        _titlePanel.SetTintColor(TitleBgColor);

        _titleText = new Text("SELECT SERVER", font, 28)
        {
            OutlineThickness = 2f,
            FillColor = TitleTextColor,
            OutlineColor = TitleTextOutline,
            Position = new Vector2f(titlePos.X + (TitleWidth * 0.5f) - 110f, titlePos.Y + 15f)
        };
    }

    #endregion Constructor

    #region Overrides

    /// <summary>
    /// Renders the title panel and text.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _titlePanel.Draw(target);
        target.Draw(_titleText);
    }

    /// <summary>
    /// Returns the title text as the main drawable.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => _titleText;

    #endregion Overrides
}