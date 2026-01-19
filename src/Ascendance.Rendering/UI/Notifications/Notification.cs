// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
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
    protected const System.Single TextCharSizePx = 20f;

    /// <summary>
    /// Horizontal padding (in pixels) inside the panel.
    /// </summary>
    protected const System.Single HorizontalPaddingPx = 12f;

    /// <summary>
    /// Vertical padding (in pixels) inside the panel.
    /// </summary>
    protected const System.Single VerticalPaddingPx = 30f;

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

    #endregion

    #region Fields

    /// <summary>
    /// SFML Text object for displaying the notification message.
    /// </summary>
    protected readonly Text _messageText;

    /// <summary>
    /// 9-Slice panel object for notification background.
    /// </summary>
    protected readonly NineSlicePanel _panel;

    /// <summary>
    /// Panel border thickness.
    /// </summary>
    protected readonly Thickness _border = new(32);

    /// <summary>
    /// Anchor position to center the text within the panel.
    /// </summary>
    protected Vector2f _textAnchor;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a notification box that displays an automatically word-wrapped message at the specified side of the screen.
    /// </summary>
    /// <param name="initialMessage">Initial message to display.</param>
    /// <param name="side">Which side of the screen to display (Top or Bottom).</param>
    public Notification(Font font, Texture frameTexture, System.Int32 zIndex, System.String initialMessage = "", Direction2D side = Direction2D.Up)
    {
        COMPUTE_LAYOUT(side, out System.Single panelY, out System.Single panelWidth, out System.Single panelX);

        _panel = CREATE_PANEL(frameTexture, panelX, panelY, panelWidth);

        System.Single innerWidth = COMPUTE_INNER_WIDTH(panelWidth);
        _messageText = PREPARE_WRAPPED_TEXT(font, initialMessage, (System.UInt32)TextCharSizePx, innerWidth);

        System.Single textHeight = CENTER_TEXT_ORIGIN_AND_MEASURE(_messageText);
        System.Single panelHeight = COMPUTE_TARGET_HEIGHT(textHeight);

        _panel.SetSize(new Vector2f(panelWidth, panelHeight));
        POSITION_TEXT_INSIDE_PANEL(_panel, textHeight, out _textAnchor);

        base.Show();
        base.SetZIndex(zIndex);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Updates the message text, maintaining the anchor position and applying word wrap.
    /// </summary>
    /// <param name="newMessage">New message to display.</param>
    public virtual void UpdateMessage(System.String newMessage)
    {
        System.Single innerWidth = COMPUTE_INNER_WIDTH(_panel.Size.X);

        System.String wrapped = WrapText(_messageText.Font, newMessage, _messageText.CharacterSize, innerWidth);
        _messageText.DisplayedString = wrapped;

        // Re-center origin but preserve anchor position
        var bounds = _messageText.GetLocalBounds();
        _messageText.Origin = new Vector2f(bounds.Left + (bounds.Width / 2f), bounds.Top + (bounds.Height / 2f));
        _messageText.Position = _textAnchor;
    }

    #endregion

    #region Overrides

    /// <inheritdoc />
    public override void Update(System.Single deltaTime)
    {
        if (!IsVisible)
        {
            return;
        }

        // No animation/state update for basic notification
    }

    /// <summary>
    /// Renders the notification panel and message onto the given target.
    /// </summary>
    /// <param name="target">Render target.</param>
    public void Render(RenderTarget target)
    {
        if (!IsVisible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_messageText);
    }

    /// <summary>
    /// Not supported for <see cref="Notification"/>. Use <see cref="Render(RenderTarget)"/> instead.
    /// </summary>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout Construction

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
        => new(WrapText(font, message, charSize, innerWidth), font, charSize) { FillColor = Color.Black };

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
    /// Positions the <see cref="_messageText"/> centered within the inner panel bounds and computes anchor position.
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

        _messageText.Position = new Vector2f(innerCenterX, innerTop + (textHeight * 0.5f));
        anchorOut = _messageText.Position;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Performs word wrapping on the specified text, splitting into multiple lines so each fits in <paramref name="maxWidth"/>.
    /// Uses a single <see cref="Text"/> instance for measuring, avoiding performance overhead.
    /// </summary>
    /// <param name="font">Font to use for measurement.</param>
    /// <param name="text">Text content to wrap.</param>
    /// <param name="characterSize">Font character size.</param>
    /// <param name="maxWidth">Maximum allowed line width.</param>
    /// <returns>Word-wrapped text.</returns>
    protected static System.String WrapText(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
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

    /// <summary>
    /// Linearly interpolates between two colors using interpolation parameter <paramref name="t"/>.
    /// </summary>
    /// <param name="a">Start color.</param>
    /// <param name="b">End color.</param>
    /// <param name="t">Interpolation coefficient [0..1].</param>
    /// <returns>Interpolated color value.</returns>
    protected static Color Lerp(Color a, Color b, System.Single t)
    {
        System.Byte LerpComponent(System.Byte start, System.Byte end) => (System.Byte)(start + ((end - start) * t));
        return new Color(
            LerpComponent(a.R, b.R),
            LerpComponent(a.G, b.G),
            LerpComponent(a.B, b.B),
            LerpComponent(a.A, b.A));
    }

    #endregion
}