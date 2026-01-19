// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Components.Notifications;

/// <summary>
/// Notification box with a single central action button.
/// Supports hover/click visual effects and automatically closes when the button is clicked.
/// </summary>
public sealed class ButtonNotification : Notification
{
    #region Constants

    /// <summary>
    /// Vertical gap (in pixels) between message text and the button.
    /// </summary>
    private const System.Single VerticalGapPx = 6f;

    /// <summary>
    /// Font size for button text (in pixels).
    /// </summary>
    private const System.Single ButtonTextSizePx = 18f;

    /// <summary>
    /// Vertical padding for the button (in pixels).
    /// </summary>
    private const System.Single ButtonPadYPx = 6f;

    /// <summary>
    /// Scale ratio for button panel sizing.
    /// </summary>
    private const System.Single ButtonScaleRatio = 0.5f;

    /// <summary>
    /// Minimum button height (in pixels).
    /// </summary>
    private const System.Single MinButtonHeightPx = 28f;

    /// <summary>
    /// Duration for hover fade-in animation (seconds).
    /// </summary>
    private const System.Single HoverFadeInSec = 0.08f;

    /// <summary>
    /// Duration for hover fade-out animation (seconds).
    /// </summary>
    private const System.Single HoverFadeOutSec = 0.10f;

    /// <summary>
    /// Minimum internal width for button content (in pixels).
    /// </summary>
    private const System.Single MinInnerWidthPx = 50f;

    #endregion

    #region Fields

    /// <summary>
    /// Nine-slice panel representing the button background.
    /// </summary>
    private readonly NineSlicePanel _buttonPanel;

    /// <summary>
    /// SFML Text for the button caption.
    /// </summary>
    private readonly Text _buttonText;

    /// <summary>
    /// Hover state flag.
    /// </summary>
    private System.Boolean _isHovering;

    /// <summary>
    /// Animation parameter for hover/fade.
    /// </summary>
    private System.Single _hoverAnim;

    /// <summary>
    /// Action callback invoked when the button is clicked.
    /// </summary>
    private event System.Action OnClicked;

    /// <summary>
    /// Button base color.
    /// </summary>
    private readonly Color _baseGray = new(220, 220, 220, 255);

    /// <summary>
    /// Button color when hovered.
    /// </summary>
    private readonly Color _hoverGray = new(120, 120, 120, 255);

    /// <summary>
    /// Texture used for the button panel.
    /// </summary>
    private readonly Texture _texture;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes an action notification box with a button and given message.
    /// </summary>
    /// <param name="initialMessage">Message text.</param>
    /// <param name="side">Side of the screen to display (Top/Bottom).</param>
    /// <param name="buttonText">Button caption.</param>
    public ButtonNotification(Font font, Texture frameTexture, System.Int32 zIndex, System.String initialMessage = "", Direction2D side = Direction2D.Down, System.String buttonText = "OK")
        : base(font, frameTexture, zIndex, initialMessage, side)
    {
        _texture = frameTexture;
        _buttonText = CREATE_BUTTON_TEXT(buttonText);
        _buttonPanel = CREATE_BUTTON_PANEL();

        SIZE_BUTTON_TO_CONTENT();
        POSITION_BUTTON();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Additional Y offset for button after layout, lets you adjust vertical spacing.
    /// </summary>
    public System.Single ButtonExtraOffsetY { get; set; }

    /// <summary>
    /// Registers a callback to be invoked when the button is clicked.
    /// </summary>
    /// <param name="handler">Action delegate to execute.</param>
    public void RegisterAction(System.Action handler) => OnClicked += handler;

    /// <summary>
    /// Unregisters a previously registered action callback.
    /// </summary>
    /// <param name="handler">Action delegate to remove.</param>
    public void UnregisterAction(System.Action handler) => OnClicked -= handler;

    #endregion

    #region Internal Logic

    /// <summary>
    /// Updates the notification message and repositions the button accordingly.
    /// </summary>
    /// <param name="newMessage">New message to display.</param>
    public override void UpdateMessage(System.String newMessage)
    {
        base.UpdateMessage(newMessage);
        POSITION_BUTTON();
    }

    /// <summary>
    /// Per-frame update: handles hover/fade/click on button.
    /// </summary>
    /// <param name="deltaTime">Elapsed seconds since last update.</param>
    public override void Update(System.Single deltaTime)
    {
        if (!IsVisible)
        {
            return;
        }

        UPDATE_HOVER_STATE(deltaTime);
        UPDATE_BUTTON_VISUALS();
        HANDLE_CLICK();
    }

    /// <summary>
    /// Renders base notification and button components.
    /// </summary>
    /// <param name="target">SFML render target.</param>
    public new void Render(RenderTarget target)
    {
        if (!IsVisible)
        {
            return;
        }

        base.Render(target);
        target.Draw(_buttonPanel);
        target.Draw(_buttonText);
    }

    #endregion

    #region Layout/Helpers

    /// <summary>
    /// Creates and configures SFML Text for button caption.
    /// </summary>
    /// <param name="caption">Button text.</param>
    /// <returns>Centered Text instance.</returns>
    private Text CREATE_BUTTON_TEXT(System.String caption)
    {
        var text = new Text(caption, _messageText.Font, (System.UInt32)ButtonTextSizePx)
        {
            FillColor = Color.Black
        };
        var bounds = text.GetLocalBounds();
        text.Origin = new Vector2f(bounds.Left + (bounds.Width / 2f), bounds.Top + (bounds.Height / 2f) - 10);
        return text;
    }

    /// <summary>
    /// Creates the button's nine-slice panel background.
    /// </summary>
    /// <returns>Button background panel.</returns>
    private NineSlicePanel CREATE_BUTTON_PANEL()
    {
        _texture.Smooth = false;

        NineSlicePanel panel = new NineSlicePanel(_texture, _border)
            .SetPosition(new Vector2f(0f, 0f))
            .SetSize(new Vector2f(200f, 64f));

        return panel;
    }

    /// <summary>
    /// Computes center X and usable width for button layout inside notification panel.
    /// </summary>
    /// <returns>(innerCenterX, innerWidth)</returns>
    private (System.Single innerCenterX, System.Single innerWidth) COMPUTE_INNER_METRICS()
    {
        System.Single innerLeft = _panel.Position.X + _border.Left + HorizontalPaddingPx;
        System.Single innerRight = _panel.Position.X + _panel.Size.X - _border.Right - HorizontalPaddingPx;
        System.Single innerCenterX = (innerLeft + innerRight) / 2f;

        System.Single innerWidth = _panel.Size.X - (_border.Left + _border.Right) - (HorizontalPaddingPx * 2f);
        innerWidth = System.MathF.Max(innerWidth, MinInnerWidthPx);

        return (innerCenterX, innerWidth);
    }

    /// <summary>
    /// Sizes the button panel to fit its text.
    /// </summary>
    private void SIZE_BUTTON_TO_CONTENT()
    {
        var (_, innerWidth) = COMPUTE_INNER_METRICS();
        var bounds = _buttonText.GetLocalBounds();

        System.Single targetW = innerWidth;
        System.Single targetH = bounds.Height + (ButtonPadYPx * 2f) + _border.Top + _border.Bottom;
        targetH = System.MathF.Max(targetH, MinButtonHeightPx);

        _buttonPanel.SetSize(new Vector2f(targetW * (ButtonScaleRatio - 0.1f), targetH * ButtonScaleRatio));
    }

    /// <summary>
    /// Positions the button panel and text under the message, vertically centered.
    /// </summary>
    private void POSITION_BUTTON()
    {
        var (innerCenterX, _) = COMPUTE_INNER_METRICS();
        var textGB = _messageText.GetGlobalBounds();

        // Optionally adjust message text position if needed
        _messageText.Position = new Vector2f(_messageText.Position.X, _messageText.Position.Y - 20);

        System.Single buttonY = textGB.Top + textGB.Height + VerticalGapPx - 200 + ButtonExtraOffsetY;
        System.Single btnX = innerCenterX - (_buttonPanel.Size.X / 2f);

        _buttonPanel.SetPosition(new Vector2f(btnX, buttonY));

        // Center the caption in panel
        System.Single btnCenterX = btnX + (_buttonPanel.Size.X / 2f);
        System.Single btnCenterY = buttonY + (_buttonPanel.Size.Y / 2f);
        _buttonText.Position = new Vector2f(btnCenterX, btnCenterY);
    }

    #endregion

    #region Interaction/Visuals

    /// <summary>
    /// Gets the rectangular bounds of the button panel for hit testing.
    /// </summary>
    /// <returns>FloatRect representing button bounds.</returns>
    private FloatRect GET_BUTTON_RECT()
        => new(_buttonPanel.Position.X, _buttonPanel.Position.Y,
               _buttonPanel.Size.X, _buttonPanel.Size.Y);

    /// <summary>
    /// Updates button hover state and animation value per frame.
    /// </summary>
    /// <param name="dt">Elapsed seconds.</param>
    private void UPDATE_HOVER_STATE(System.Single dt)
    {
        Vector2i mouse = MouseManager.Instance.GetMousePosition();
        System.Boolean hover = GET_BUTTON_RECT().Contains(mouse.X, mouse.Y);

        if (hover)
        {
            _isHovering = true;
            _hoverAnim += dt / HoverFadeInSec;
        }
        else
        {
            _isHovering = false;
            _hoverAnim -= dt / HoverFadeOutSec;
        }

        _hoverAnim = System.Math.Clamp(_hoverAnim, 0f, 1f);
    }

    /// <summary>
    /// Updates button color and text color based on current hover animation state.
    /// </summary>
    private void UPDATE_BUTTON_VISUALS()
    {
        _buttonPanel.SetTintColor(LERP(_baseGray, _hoverGray, _hoverAnim));
        _buttonText.FillColor = LERP(Color.Black, Color.White, _hoverAnim);
    }

    /// <summary>
    /// Handles left mouse button click over button, fires action and closes the notification.
    /// </summary>
    private void HANDLE_CLICK()
    {
        if (_isHovering && MouseManager.Instance.IsMouseButtonPressed(Mouse.Button.Left))
        {
            OnClicked?.Invoke();
            Hide();
        }
    }

    /// <summary>
    /// Linear color interpolation for hover effect.
    /// </summary>
    /// <param name="a">Start color.</param>
    /// <param name="b">End color.</param>
    /// <param name="t">Lerp coefficient [0..1].</param>
    /// <returns>Interpolated color.</returns>
    private static Color LERP(Color a, Color b, System.Single t)
    {
        t = System.Math.Clamp(t, 0f, 1f);
        System.Byte LerpByte(System.Byte x, System.Byte y) => (System.Byte)(x + ((y - x) * t));
        return new Color(
            LerpByte(a.R, b.R),
            LerpByte(a.G, b.G),
            LerpByte(a.B, b.B),
            LerpByte(a.A, b.A));
    }

    #endregion
}