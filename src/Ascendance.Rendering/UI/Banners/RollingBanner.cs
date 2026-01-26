// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Banners;

/// <summary>
/// A continuously scrolling banner displaying a sequence of messages from right to left.
/// Supports displaying multiple messages in succession, cycling them when they scroll past the left edge.
/// </summary>
public class RollingBanner : RenderObject
{
    #region Constants

    /// <summary>
    /// Vertical offset (in pixels) for the text inside the banner.
    /// </summary>
    private const System.Single TextOffsetYPx = 4f;

    /// <summary>
    /// Banner height in pixels.
    /// </summary>
    private const System.Single BannerHeightPx = 32f;

    /// <summary>
    /// Horizontal gap (in pixels) between adjacent messages.
    /// </summary>
    private const System.Single TextGapPx = 50f;

    /// <summary>
    /// Default font size in pixels.
    /// </summary>
    private const System.UInt32 FontSizePx = 18u;

    /// <summary>
    /// Default text color (white, opaque).
    /// </summary>
    private static readonly Color DefaultTextColor = new(255, 255, 255);

    /// <summary>
    /// Default banner background color (black, alpha 100).
    /// </summary>
    private static readonly Color BackgroundColor = new(0, 0, 0, 100);

    /// <summary>
    /// Vector representing leftward scroll.
    /// </summary>
    private static readonly Vector2f ScrollDirection = new(-1f, 0f);

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
    /// <param name="messages">The collection of messages to display.</param>
    /// <param name="speedPxPerSec">The scroll speed in pixels per second. Default is 100.</param>
    public RollingBanner(System.Collections.Generic.List<System.String> messages, System.Int32 zIndex, Font font, System.Single speedPxPerSec = 100f)
    {
        this.SetZIndex(zIndex);
        this.Show();

        this._font = font;
        this._speedPxPerSec = speedPxPerSec;
        this._background = CREATE_BACKGROUND();

        this.INITIALIZE_TEXTS(messages);
    }

    #endregion Constructors

    #region Public API

    /// <summary>
    /// Updates the message list and resets all text positions.
    /// </summary>
    /// <param name="messages">The new list of messages to display.</param>
    public void SetMessages(System.Collections.Generic.List<System.String> messages)
    {
        this._texts.Clear();
        this.INITIALIZE_TEXTS(messages);
    }

    #endregion Public API

    #region Overrides

    /// <inheritdoc/>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible || this._texts.Count == 0)
        {
            return;
        }

        this.SCROLL_TEXTS(deltaTime);

        Text first = this._texts[0];
        if (first.Position.X + first.GetGlobalBounds().Width < 0)
        {
            Text last = this._texts[^1];
            first.Position = new Vector2f(
                last.Position.X + last.GetGlobalBounds().Width + TextGapPx,
                first.Position.Y
            );

            this._texts.RemoveAt(0);
            this._texts.Add(first);
        }
    }

    /// <summary>
    /// Renders the banner on the specified render target.
    /// </summary>
    /// <param name="target">The render target for drawing.</param>
    public void Render(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(this._background);
        foreach (var text in this._texts)
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
            FillColor = BackgroundColor,
            Size = new Vector2f(GraphicsEngine.ScreenSize.X, BannerHeightPx),
            Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - BannerHeightPx),
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
            var text = CREATE_TEXT(msg, this._font, startX);
            this._texts.Add(text);

            startX += text.GetGlobalBounds().Width + TextGapPx;
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
        return new Text(message, font, FontSizePx)
        {
            FillColor = DefaultTextColor,
            Position = new Vector2f(startX, GraphicsEngine.ScreenSize.Y - BannerHeightPx + TextOffsetYPx)
        };
    }

    /// <summary>
    /// Scrolls all messages left based on configured speed and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    private void SCROLL_TEXTS(System.Single deltaTime)
    {
        System.Single displacement = this._speedPxPerSec * deltaTime;
        for (System.Int32 i = 0; i < this._texts.Count; i++)
        {
            this._texts[i].Position += ScrollDirection * displacement;
        }
    }

    #endregion Private Helpers
}