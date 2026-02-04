// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Notifications;

/// <summary>
/// Lightweight notification box (without buttons), rendering a 9-slice panel background and automatically word-wrapped text.
/// </summary>
public class Notification : RenderObject
{
    #region Constants

    /// <summary>
    /// Default character size for notification text (in pixels).
    /// </summary>
    private const System.Single TextCharSizePx = 20f;

    /// <summary>
    /// Horizontal padding (in pixels) inside the panel.
    /// </summary>
    private const System.Single HorizontalPaddingPx = 12f;

    /// <summary>
    /// Vertical padding (in pixels) inside the panel.
    /// </summary>
    private const System.Single VerticalPaddingPx = 30f;

    /// <summary>
    /// Relative Y position when anchored to top of the screen.
    /// </summary>
    private const System.Single TopYRatio = 0.10f;

    /// <summary>
    /// Relative Y position when anchored to bottom of the screen.
    /// </summary>
    private const System.Single BottomYRatio = 0.70f;

    /// <summary>
    /// Maximum width as a fraction of screen width.
    /// </summary>
    private const System.Single MaxWidthFraction = 0.85f;

    /// <summary>
    /// Absolute maximum notification width (in pixels).
    /// </summary>
    private const System.Single MaxWidthCapPx = 720f;

    /// <summary>
    /// Initial panel height (in pixels, before calculating actual text size).
    /// </summary>
    private const System.Single InitialPanelHeightPx = 64f;

    /// <summary>
    /// Minimum allowed panel height (in pixels).
    /// </summary>
    private const System.Single MinPanelHeightPx = 162f;

    /// <summary>
    /// Minimum allowed text inner width (in pixels) inside the panel.
    /// </summary>
    private const System.Single MinInnerWidthPx = 50f;

    #endregion Constants

    #region Fields

    private Vector2f _textAnchor;
    private readonly Thickness _border = new(32);

    #endregion Fields

    #region Properties

    /// <summary>
    /// Message text object.
    /// </summary>
    protected readonly Text MessageText;

    /// <summary>
    /// Panel background object.
    /// </summary>
    protected readonly NineSlicePanel Panel;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a notification box that displays an automatically word-wrapped message at the specified side of the screen.
    /// </summary>
    /// <param name="initialMessage">Initial message to display.</param>
    /// <param name="side">Which side of the screen to display (Top or Bottom).</param>
    public Notification(Texture frameTexture, System.String initialMessage = "", Direction2D side = Direction2D.Up, Font font = null)
    {
        COMPUTE_LAYOUT(side, out System.Single panelY, out System.Single panelWidth, out System.Single panelX);

        this.Panel = this.CREATE_PANEL(frameTexture, panelX, panelY, panelWidth);

        System.Single innerWidth = COMPUTE_INNER_WIDTH(panelWidth);
        this.MessageText = PREPARE_WRAPPED_TEXT(font ?? EmbeddedAssets.JetBrainsMono.ToFont(), initialMessage, (System.UInt32)TextCharSizePx, innerWidth);

        System.Single textHeight = CENTER_TEXT_ORIGIN_AND_MEASURE(this.MessageText);
        System.Single panelHeight = COMPUTE_TARGET_HEIGHT(textHeight);

        this.Panel.SetSize(new Vector2f(panelWidth, panelHeight));
        this.POSITION_TEXT_INSIDE_PANEL(this.Panel, textHeight, out _textAnchor);

        base.Show();
        base.SetZIndex(RenderLayer.Notification.ToZIndex());
    }

    #endregion Constructors

    #region Public API

    /// <summary>
    /// Updates the message text, maintaining the anchor position and applying word wrap.
    /// </summary>
    /// <param name="newMessage">New message to display.</param>
    public virtual void UpdateMessage(System.String newMessage)
    {
        this.MessageText.DisplayedString = WRAP_TEXT(this.MessageText.Font, newMessage, this.MessageText.CharacterSize, COMPUTE_INNER_WIDTH(this.Panel.Size.X));

        // Re-center origin but preserve anchor position
        FloatRect bounds = this.MessageText.GetLocalBounds();
        this.MessageText.Position = _textAnchor;
        this.MessageText.Origin = new Vector2f(bounds.Left + (bounds.Width / 2f), bounds.Top + (bounds.Height / 2f));
    }

    #endregion Public API

    #region Overrides

    /// <inheritdoc />
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible)
        {
            return;
        }

        // No animation/state update for basic notification
    }

    /// <summary>
    /// Renders the notification panel and message onto the given target.
    /// </summary>
    /// <param name="target">Render target.</param>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        Panel.Draw(target);
        target.Draw(MessageText);
    }

    /// <summary>
    /// Not supported for <see cref="Notification"/>. Use <see cref="Draw(RenderTarget)"/> instead.
    /// </summary>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// Calculates panel position and size depending on screen side.
    /// </summary>
    /// <param name="side">Top or Bottom side of screen.</param>
    /// <param name="panelY">Y position of panel.</param>
    /// <param name="panelWidth">Width of panel.</param>
    /// <param name="panelX">X position of panel.</param>
    private static void COMPUTE_LAYOUT(
        Direction2D side,
        out System.Single panelY,
        out System.Single panelWidth,
        out System.Single panelX)
    {
        System.Single ratio = side == Direction2D.Down ? BottomYRatio : TopYRatio;
        System.Single screenW = GraphicsEngine.ScreenSize.X;

        System.Single rawWidth = screenW * MaxWidthFraction;
        panelWidth = System.MathF.Min(rawWidth, MaxWidthCapPx);

        panelX = (screenW - panelWidth) / 2f;
        panelY = GraphicsEngine.ScreenSize.Y * ratio;
    }

    /// <summary>
    /// Creates nine-slice panel background.
    /// </summary>
    /// <param name="frameTexture">Background texture.</param>
    /// <param name="x">Panel X position.</param>
    /// <param name="y">Panel Y position.</param>
    /// <param name="width">Panel width.</param>
    /// <returns>NineSlicePanel instance.</returns>
    private NineSlicePanel CREATE_PANEL(Texture frameTexture, System.Single x, System.Single y, System.Single width)
    {
        NineSlicePanel panel = new NineSlicePanel(frameTexture, _border)
            .SetPosition(new Vector2f(x, y))
            .SetSize(new Vector2f(width, InitialPanelHeightPx)); // Temporary height

        return panel;
    }

    /// <summary>
    /// Calculates inner text width based on panel width and padding.
    /// </summary>
    /// <param name="panelWidth">Panel width.</param>
    /// <returns>Usable width for text rendering.</returns>
    private static System.Single COMPUTE_INNER_WIDTH(System.Single panelWidth)
        => System.MathF.Max(MinInnerWidthPx, panelWidth - (2f * HorizontalPaddingPx));

    /// <summary>
    /// Prepares SFML Text object with word-wrapped message.
    /// </summary>
    /// <param name="font">Font object.</param>
    /// <param name="message">String to display.</param>
    /// <param name="charSize">Font size.</param>
    /// <param name="innerWidth">Max width for wrapping.</param>
    /// <returns>Configured Text object.</returns>
    private static Text PREPARE_WRAPPED_TEXT(Font font, System.String message, System.UInt32 charSize, System.Single innerWidth)
        => new(WRAP_TEXT(font, message, charSize, innerWidth), font, charSize) { FillColor = Color.Black };

    /// <summary>
    /// Re-centers origin of Text and returns measured height.
    /// </summary>
    /// <param name="text">Text object.</param>
    /// <returns>Measured height of text block.</returns>
    private static System.Single CENTER_TEXT_ORIGIN_AND_MEASURE(Text text)
    {
        var localBounds = text.GetLocalBounds();
        text.Origin = new Vector2f(localBounds.Left + (localBounds.Width / 2f), localBounds.Top + (localBounds.Height / 2f));
        return text.GetGlobalBounds().Height;
    }

    /// <summary>
    /// Computes target panel height based on text height and vertical padding.
    /// </summary>
    /// <param name="textHeight">Measured text height.</param>
    /// <returns>Panel height.</returns>
    private static System.Single COMPUTE_TARGET_HEIGHT(System.Single textHeight)
    {
        System.Single height = VerticalPaddingPx + textHeight + VerticalPaddingPx;
        return System.MathF.Max(MinPanelHeightPx, height);
    }

    /// <summary>
    /// Positions the <see cref="MessageText"/> centered within the inner panel bounds and computes anchor position.
    /// </summary>
    /// <param name="panel">Panel object.</param>
    /// <param name="textHeight">Measured text height.</param>
    /// <param name="anchorOut">Returns computed anchor position.</param>
    private void POSITION_TEXT_INSIDE_PANEL(NineSlicePanel panel, System.Single textHeight, out Vector2f anchorOut)
    {
        System.Single innerLeft = panel.Position.X + _border.Left + HorizontalPaddingPx;
        System.Single innerRight = panel.Position.X + panel.Size.X - _border.Right - HorizontalPaddingPx;
        System.Single innerCenterX = (innerLeft + innerRight) / 2f;
        System.Single innerTop = panel.Position.Y + _border.Top + VerticalPaddingPx;

        MessageText.Position = new Vector2f(innerCenterX, innerTop + (textHeight * 0.5f));
        anchorOut = MessageText.Position;
    }

    /// <summary>
    /// Performs word wrapping on the specified text, splitting into multiple lines so each fits in <paramref name="maxWidth"/>.
    /// Uses a single <see cref="Text"/> instance for measuring, avoiding performance overhead.
    /// </summary>
    /// <param name="font">Font to use for measurement.</param>
    /// <param name="text">Text content to wrap.</param>
    /// <param name="characterSize">Font character size.</param>
    /// <param name="maxWidth">Maximum allowed line width.</param>
    /// <returns>Word-wrapped text.</returns>
    private static System.String WRAP_TEXT(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
    {
        if (System.String.IsNullOrEmpty(text))
        {
            return System.String.Empty;
        }

        System.String result = "";
        System.String currentLine = "";
        System.String[] words = text.Split(' ');

        var measurer = new Text("", font, characterSize);

        foreach (System.String word in words)
        {
            System.String testLine = System.String.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            measurer.DisplayedString = testLine;
            if (measurer.GetLocalBounds().Width > maxWidth)
            {
                if (!System.String.IsNullOrEmpty(currentLine))
                {
                    result += currentLine + "\n";
                    currentLine = word;
                }
                else
                {
                    // Word longer than maxWidth: force wrapping on this word
                    result += word + "\n";
                    currentLine = "";
                }
            }
            else
            {
                currentLine = testLine;
            }
        }

        result += currentLine;
        return result;
    }

    #endregion Private Methods
}