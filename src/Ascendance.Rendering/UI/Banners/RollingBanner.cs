// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.UI.Theme;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Banners;

/// <summary>
/// Represents a horizontally scrolling banner that continuously displays
/// a sequence of messages from right to left.
/// </summary>
/// <remarks>
/// Messages are rendered sequentially and recycled once they move past
/// the left edge of the screen, creating a seamless rolling effect.
/// </remarks>
public class RollingBanner : RenderObject
{
    #region Constants

    /// <summary>
    /// Banner height in pixels.
    /// </summary>
    private const System.Single BannerHeight = 32f;

    /// <summary>
    /// Horizontal gap (in pixels) between adjacent messages.
    /// </summary>
    private const System.Single MessageSpacing = 50f;

    /// <summary>
    /// Default font size in pixels.
    /// </summary>
    private const System.UInt32 DefaultFontSize = 18u;

    /// <summary>
    /// Vertical offset (in pixels) for the text inside the banner.
    /// </summary>
    private const System.Single TextVerticalOffset = 4f;

    /// <summary>
    /// Vector representing leftward scroll.
    /// </summary>
    private static readonly Vector2f ScrollLeftDirection = new(-1f, 0f);

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly RectangleShape _background;
    private readonly System.Single _speedPxPerSec;
    private readonly System.Collections.Generic.List<Text> _texts = [];

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingBanner"/> class.
    /// </summary>
    /// <param name="messages">
    /// The initial collection of messages to display in the banner.
    /// </param>
    /// <param name="font">
    /// The font used to render banner text.
    /// </param>
    /// <param name="speedPxPerSec">
    /// The horizontal scrolling speed in pixels per second.
    /// </param>
    public RollingBanner(System.Collections.Generic.List<System.String> messages, Font font = null, System.Single speedPxPerSec = 100f)
    {
        this.Show();

        _speedPxPerSec = speedPxPerSec;
        _background = CREATE_BACKGROUND();
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();

        this.INITIALIZE_TEXTS(messages);
        base.SetZIndex(RenderLayer.Banner.ToZIndex());
    }

    #endregion Constructors

    #region Public API

    /// <summary>
    /// Updates the message list and resets all text positions.
    /// </summary>
    /// <param name="messages">The new list of messages to display.</param>
    public void SetMessages(System.Collections.Generic.List<System.String> messages)
    {
        _texts.Clear();
        this.INITIALIZE_TEXTS(messages);
    }

    #endregion Public API

    #region Overrides

    /// <summary>
    /// Updates the banner animation and scrolls messages based on elapsed time.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time, in seconds, since the previous frame.
    /// </param>
    /// <remarks>
    /// When a message scrolls completely past the left edge of the screen,
    /// it is repositioned to the end of the message sequence.
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible || _texts.Count == 0)
        {
            return;
        }

        this.SCROLL_TEXTS(deltaTime);

        Text first = _texts[0];
        if (first.Position.X + first.GetGlobalBounds().Width < 0)
        {
            Text last = _texts[^1];
            first.Position = new Vector2f(last.Position.X + last.GetGlobalBounds().Width + MessageSpacing, first.Position.Y);

            _texts.RemoveAt(0);
            _texts.Add(first);
        }
    }

    /// <summary>
    /// Renders the banner background and scrolling text to the specified render target.
    /// </summary>
    /// <param name="target">
    /// The render target on which the banner will be drawn.
    /// </param>
    public void Render(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);
        foreach (Text text in _texts)
        {
            target.Draw(text);
        }
    }

    /// <summary>
    /// Not supported for <see cref="RollingBanner"/>. Use <see cref="Render(RenderTarget)"/> instead.
    /// </summary>
    /// <returns>No return; always throws.</returns>
    /// <exception cref="System.NotSupportedException"></exception>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");

    #endregion Overrides

    #region Private Helpers

    /// <summary>
    /// Creates the banner's background rectangle.
    /// </summary>
    /// <returns>A <see cref="RectangleShape"/> configured as banner background.</returns>
    private static RectangleShape CREATE_BACKGROUND()
    {
        return new RectangleShape
        {
            FillColor = Themes.BannerBackgroundColor,
            Size = new Vector2f(GraphicsEngine.ScreenSize.X, BannerHeight),
            Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - BannerHeight),
        };
    }

    /// <summary>
    /// Initializes the message texts and arranges them horizontally for seamless scrolling.
    /// </summary>
    /// <param name="messages">The list of messages.</param>
    private void INITIALIZE_TEXTS(System.Collections.Generic.List<System.String> messages)
    {
        System.Single startX = GraphicsEngine.ScreenSize.X;
        foreach (System.String msg in messages)
        {
            Text text = CREATE_TEXT(msg, _font, startX);
            _texts.Add(text);

            startX += text.GetGlobalBounds().Width + MessageSpacing;
        }
    }

    /// <summary>
    /// Creates a <see cref="Text"/> SFML object with default style and specified horizontal position.
    /// </summary>
    /// <param name="message">The message string to display.</param>
    /// <param name="font">The font used to render the text.</param>
    /// <param name="startX">X coordinate for initial placement.</param>
    /// <returns>A configured <see cref="Text"/> object.</returns>
    private static Text CREATE_TEXT(System.String message, Font font, System.Single startX)
    {
        return new Text(message, font, DefaultFontSize)
        {
            FillColor = Themes.PrimaryTextColor,
            Position = new Vector2f(startX, GraphicsEngine.ScreenSize.Y - BannerHeight + TextVerticalOffset)
        };
    }

    /// <summary>
    /// Scrolls all messages left based on configured speed and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    private void SCROLL_TEXTS(System.Single deltaTime)
    {
        System.Single displacement = _speedPxPerSec * deltaTime;
        for (System.Int32 i = 0; i < _texts.Count; i++)
        {
            _texts[i].Position += ScrollLeftDirection * displacement;
        }
    }

    #endregion Private Helpers
}