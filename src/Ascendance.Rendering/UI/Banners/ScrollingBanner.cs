// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.UI.Theme;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Banners;

/// <summary>
/// Represents a horizontally scrolling banner that displays a single message
/// moving from right to left across the screen.
/// </summary>
/// <remarks>
/// Once the message has completely exited the left edge of the screen,
/// it is repositioned to the right edge and continues scrolling indefinitely.
/// </remarks>
public class ScrollingBanner : RenderObject, IUpdatable
{
    #region Constants

    /// <summary>
    /// Height of the banner (in pixels).
    /// </summary>
    private const System.Single BannerHeight = 32f;

    /// <summary>
    /// Font size (in pixels).
    /// </summary>
    private const System.UInt32 DefaultFontSize = 18u;

    /// <summary>
    /// Vertical offset (in pixels) for text inside the banner.
    /// </summary>
    private const System.Single TextVerticalOffset = 4f;

    /// <summary>
    /// The direction vector for scrolling (leftwards).
    /// </summary>
    private static readonly Vector2f ScrollLeftDirection = new(-1f, 0f);

    #endregion Constants

    #region Fields

    private readonly Text _text;
    private readonly RectangleShape _background;
    private readonly System.Single _speedPxPerSec;

    private System.Single _textWidthPx;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingBanner"/> class.
    /// </summary>
    /// <param name="message">
    /// The message to display in the banner.
    /// </param>
    /// <param name="font">
    /// The font used to render the banner text.
    /// </param>
    /// <param name="speedPxPerSec">
    /// The horizontal scrolling speed in pixels per second.
    /// </param>
    public ScrollingBanner(System.String message, Font font, System.Single speedPxPerSec = 100f)
    {
        _speedPxPerSec = speedPxPerSec;
        _background = CREATE_BACKGROUND();
        _text = CREATE_TEXT(message, font);

        base.Show();
        this.SetMessage(message);
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Sets the banner message and resets its scroll position.
    /// </summary>
    /// <param name="message">The new message to display.</param>
    public void SetMessage(System.String message)
    {
        _text.DisplayedString = message;
        _textWidthPx = _text.GetGlobalBounds().Width;

        this.RESET_TEXT_POSITION();
    }

    /// <summary>
    /// Renders the scrolling banner (background and message) onto the given render target.
    /// </summary>
    /// <param name="target">The render target.</param>
    public void Render(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);
        target.Draw(_text);
    }

    #endregion Public Methods

    #region Overrides

    /// <inheritdoc />
    /// <remarks>
    /// When the text has fully moved off the left edge of the screen,
    /// it is repositioned to the right edge to continue scrolling.
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.MOVE_TEXT(deltaTime);

        if (_text.Position.X + _textWidthPx < 0)
        {
            _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, _text.Position.Y);
        }
    }

    /// <summary>
    /// This method is not supported for ScrollingBanner. Use <see cref="Render(RenderTarget)"/> instead.
    /// </summary>
    /// <returns>Never returns normally.</returns>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Please use Render() instead of GetDrawable().");

    #endregion Overrides

    #region Private Helpers

    /// <summary>
    /// Creates and configures the banner's background shape.
    /// </summary>
    /// <returns>A new <see cref="RectangleShape"/> for the banner background.</returns>
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
    /// Creates and configures the banner's text object.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="font">The font to use.</param>
    /// <returns>A new <see cref="Text"/> instance.</returns>
    private static Text CREATE_TEXT(System.String message, Font font)
    {
        return new Text(message, font, DefaultFontSize)
        {
            FillColor = Themes.PrimaryTextColor,
        };
    }

    /// <summary>
    /// Resets the text position to start scrolling in from the right edge.
    /// </summary>
    private void RESET_TEXT_POSITION() => _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y - BannerHeight + TextVerticalOffset);

    /// <summary>
    /// Moves the text leftwards according to current speed and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time (in seconds) since last update.</param>
    private void MOVE_TEXT(System.Single deltaTime) => _text.Position += ScrollLeftDirection * (_speedPxPerSec * deltaTime);

    #endregion Private Helpers
}