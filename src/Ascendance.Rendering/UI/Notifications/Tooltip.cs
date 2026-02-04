// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Layout;
using Ascendance.Rendering.UI.Theme;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Notifications;

/// <summary>
/// Represents a tooltip UI control that displays a description
/// or hint when hovered over or explicitly requested.
/// </summary>
/// <remarks>
/// Displays a small popup with short explanatory text.
/// Appears on mouse hover or can be shown programmatically at a specific position.
/// </remarks>
public class Tooltip : RenderObject
{
    #region Constants

    private const System.Single PaddingX = 12f;
    private const System.Single PaddingY = 8f;
    private const System.Single MaxWidthFraction = 0.4f;
    private const System.Single ShowDelay = 0.15f;
    private const System.Single MinWidth = 60f;

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly NineSlicePanel _panel;
    private readonly Text _tipText;
    private readonly Thickness _border = new(18);

    private System.String _message;
    private System.Single _maxWidth;
    private System.Single _showTimer;
    private System.Boolean _pendingShow;
    private System.Boolean _visibleThisFrame;

    private Vector2f _anchor;
    private System.Boolean _followMouse;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the tooltip's foreground (text) color.
    /// </summary>
    public Color Foreground { get; set; } = Themes.PrimaryTextColor;

    /// <summary>
    /// Gets or sets the tooltip background color.
    /// </summary>
    public Color Background { get; set; } = new(40, 40, 44, 235);

    /// <summary>
    /// Gets or sets the maximum width of the tooltip (in pixels).
    /// Setting a value less than the minimum width will use the minimum width.
    /// </summary>
    public System.Single MaxWidth
    {
        get => _maxWidth;
        set
        {
            _maxWidth = value < MinWidth ? MinWidth : value;
            UPDATE_TOOLTIP_LAYOUT();
        }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Tooltip"/> class with optional font and max width.
    /// </summary>
    /// <param name="font">
    /// The font for displaying tooltip text. If null, a default font will be used.
    /// </param>
    /// <param name="maxWidth">
    /// The maximum width of the tooltip in pixels. If null, will default to 40% of screen width.
    /// </param>
    public Tooltip(Font font = null, System.Single? maxWidth = null)
    {
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();
        _maxWidth = maxWidth ?? (GraphicsEngine.ScreenSize.X * MaxWidthFraction);
        _tipText = new Text("", _font, 16) { FillColor = Foreground };
        _panel = new NineSlicePanel(EmbeddedAssets.SquareOutline.ToTexture(), _border);

        _anchor = default;
        _followMouse = true;
        base.Hide();
        base.SetZIndex(RenderLayer.Tooltip.ToZIndex());
    }

    #endregion Constructor

    #region Public API

    /// <summary>
    /// Shows the tooltip with the specified message at the mouse position
    /// or at a specific anchor coordinate.
    /// </summary>
    /// <param name="message">
    /// The message to display in the tooltip.
    /// </param>
    /// <param name="anchor">
    /// The screen coordinate to anchor the tooltip to.
    /// If null, the tooltip follows the mouse position.
    /// </param>
    public void Show(System.String message, Vector2f? anchor = null)
    {
        _message = message;
        _pendingShow = true;
        _showTimer = 0f;
        _anchor = anchor ?? default;
        _followMouse = anchor == null;
        _visibleThisFrame = true;
    }

    /// <summary>
    /// Hides the tooltip immediately.
    /// </summary>
    public new void Hide()
    {
        base.Hide();
        _visibleThisFrame = false;
        _pendingShow = false;
        _showTimer = 0f;
    }

    #endregion Public API

    #region Main Loop

    /// <summary>
    /// Updates the tooltip's display state and position.
    /// Should be called every frame.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time since the last update, in seconds.
    /// </param>
    public override void Update(System.Single deltaTime)
    {
        if (_pendingShow)
        {
            _showTimer += deltaTime;

            if (_showTimer >= ShowDelay)
            {
                UPDATE_TOOLTIP_LAYOUT();
                base.Show();
                _pendingShow = false;
                _visibleThisFrame = true;
            }
        }
        // Auto-hide if Show was not called this frame.
        else if (!_visibleThisFrame && this.IsVisible)
        {
            Hide();
        }
        _visibleThisFrame = false;

        // Tooltip positioning, follow mouse if needed.
        if (this.IsVisible && _followMouse)
        {
            var mp = Ascendance.Rendering.Input.MouseManager.Instance.GetMousePosition();
            var screen = GraphicsEngine.ScreenSize;
            System.Single tipW = _panel.Size.X, tipH = _panel.Size.Y;

            System.Single tipX = mp.X + 18f;
            System.Single tipY = mp.Y + 24f;

            // Prevent tooltip from overflowing the screen.
            if (tipX + tipW > screen.X - 6f)
            {
                tipX = screen.X - tipW - 6f;
            }

            if (tipY + tipH > screen.Y - 6f)
            {
                tipY = screen.Y - tipH - 6f;
            }

            _panel.SetPosition(new Vector2f(tipX, tipY));
        }
    }

    /// <summary>
    /// Draws the tooltip to the specified render target.
    /// </summary>
    /// <param name="target">
    /// The render target to draw on.
    /// </param>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_tipText);
    }

    /// <summary>
    /// Not supported for <see cref="Tooltip"/>; use <see cref="Draw(RenderTarget)"/> instead.
    /// </summary>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="System.NotSupportedException">
    /// Always thrown when this method is called.
    /// </exception>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Tooltip: use Draw() method instead.");

    #endregion Main Loop

    #region Internal

    /// <summary>
    /// Updates the layout and size of the tooltip box and text.
    /// Called internally when showing the tooltip or adjusting size.
    /// </summary>
    private void UPDATE_TOOLTIP_LAYOUT()
    {
        if (System.String.IsNullOrEmpty(_message))
        {
            return;
        }

        System.String wrapped = WRAP_TEXT(_font, _message, _tipText.CharacterSize, _maxWidth - (2f * PaddingX));
        _tipText.DisplayedString = wrapped;
        _tipText.FillColor = Foreground;

        var tb = _tipText.GetLocalBounds();

        System.Single w = tb.Width + (2f * PaddingX) + _border.Left + _border.Right;
        System.Single h = tb.Height + (2f * PaddingY) + _border.Top + _border.Bottom;

        w = w < MinWidth ? MinWidth : w;

        _panel.SetSize(new Vector2f(w, h));
        // If anchored, position panel at anchor point
        if (!_followMouse)
        {
            _panel.SetPosition(_anchor);
        }

        // Position text in the center area inside the panel
        System.Single x = _panel.Position.X + _border.Left + PaddingX;
        System.Single y = _panel.Position.Y + _border.Top + PaddingY;

        _tipText.Position = new Vector2f(x, y);
        _panel.SetTintColor(Background);
    }

    /// <summary>
    /// Performs word-wrapping for the tooltip text so it stays within the given maximum width.
    /// </summary>
    /// <param name="font">Font used for rendering.</param>
    /// <param name="text">Text to wrap.</param>
    /// <param name="characterSize">Font size in pixels.</param>
    /// <param name="maxWidth">Maximum allowed width for a line.</param>
    /// <returns>
    /// A string with line breaks to fit within the specified width.
    /// </returns>
    private static System.String WRAP_TEXT(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
    {
        System.Text.StringBuilder result = new();
        System.String[] words = text.Split(' ');
        Text measurer = new("", font, characterSize);

        System.String currentLine = "";
        foreach (System.String word in words)
        {
            System.String trial = System.String.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            measurer.DisplayedString = trial;
            if (measurer.GetLocalBounds().Width > maxWidth)
            {
                if (!System.String.IsNullOrEmpty(currentLine))
                {
                    result.AppendLine(currentLine);
                    currentLine = word;
                }
                else
                {
                    // Single word is too long for the line; force-break.
                    result.AppendLine(word);
                    currentLine = "";
                }
            }
            else
            {
                currentLine = trial;
            }
        }
        if (!System.String.IsNullOrEmpty(currentLine))
        {
            result.Append(currentLine);
        }

        return result.ToString();
    }

    #endregion
}