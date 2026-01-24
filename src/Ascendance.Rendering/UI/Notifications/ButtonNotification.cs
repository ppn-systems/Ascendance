// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Enums;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Notifications;

/// <summary>
/// Notification box with a single action button using reusable Button class.
/// Supports click visual effects and automatically closes when the button is clicked.
/// </summary>
public sealed class ButtonNotification : Notification
{
    #region Fields

    /// <summary>
    /// The action button (reused from UI.Controls.Button).
    /// </summary>
    private readonly Button _actionButton;

    /// <summary>
    /// Vertical gap (in pixels) between message text and the button.
    /// </summary>
    private const System.Single VerticalGapPx = 12f;

    /// <summary>
    /// Font size for button text (in pixels).
    /// </summary>
    private const System.UInt32 ButtonFontSize = 18;

    /// <summary>
    /// Default button width.
    /// </summary>
    private const System.Single DefaultButtonWidth = 120f;

    /// <summary>
    /// Default button height.
    /// </summary>
    private const System.Single DefaultButtonHeight = 36f;

    #endregion

    #region Properties

    /// <summary>
    /// Callback fired when button is clicked.
    /// </summary>
    private event System.Action OnClicked;

    /// <summary>
    /// Extra vertical offset for the button after layout.
    /// </summary>
    public System.Single ButtonExtraOffsetY { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a notification box with an action button under the message.
    /// </summary>
    /// <param name="font">Font for notification and button text.</param>
    /// <param name="buttonTexture">Texture for button panel.</param>
    /// <param name="zIndex">Z-layer index.</param>
    /// <param name="initialMessage">Initial notification message.</param>
    /// <param name="side">Side of the screen to display notification.</param>
    /// <param name="buttonText">Label of action button.</param>
    public ButtonNotification(
        Font font,
        Texture buttonTexture,
        System.Int32 zIndex,
        System.String initialMessage = "",
        Direction2D side = Direction2D.Down,
        System.String buttonText = "OK")
        : base(font, buttonTexture, zIndex, initialMessage, side)
    {
        // Tạo button, có thể chỉnh pad, màu tuỳ ý qua các hàm của Button.
        _actionButton = new Button(buttonText, buttonTexture, font)
            .SetFontSize(ButtonFontSize)
            .SetSize(DefaultButtonWidth, DefaultButtonHeight);

        // Đăng ký sự kiện click để đóng notification và gọi OnClicked.
        _actionButton.RegisterClickHandler(ON_BUTTON_PRESSED);

        // Lần đầu layout nút.
        UPDATE_BUTTON_LAYOUT();
    }

    #endregion

    #region Event Registration

    /// <summary>
    /// Registers a callback to be invoked when the button is clicked.
    /// </summary>
    public void RegisterAction(System.Action handler) => OnClicked += handler;

    /// <summary>
    /// Unregisters a previously registered action callback.
    /// </summary>
    public void UnregisterAction(System.Action handler) => OnClicked -= handler;

    #endregion

    #region Overrides

    /// <summary>
    /// Updates notification and its button.
    /// </summary>
    public override void Update(System.Single deltaTime)
    {
        if (!IsVisible)
        {
            return;
        }

        base.Update(deltaTime);
        _actionButton.Update(deltaTime);
    }

    /// <summary>
    /// Renders notification base and action button.
    /// </summary>
    public new void Draw(RenderTarget target)
    {
        if (!IsVisible)
        {
            return;
        }

        base.Draw(target);
        _actionButton.Draw(target);
    }

    /// <summary>
    /// Called when notification message is updated to reposition button.
    /// </summary>
    public override void UpdateMessage(System.String newMessage)
    {
        base.UpdateMessage(newMessage);
        UPDATE_BUTTON_LAYOUT();
    }

    #endregion

    #region Private Logic

    /// <summary>
    /// Lays out the action button underneath the notification message.
    /// </summary>
    private void UPDATE_BUTTON_LAYOUT()
    {
        var messageBounds = _messageText.GetGlobalBounds();

        // Trung tâm theo chiều ngang notification, bên dưới message một đoạn.
        System.Single notifWidth = _panel.Size.X;
        System.Single btnWidth = DefaultButtonWidth;
        System.Single btnX = _panel.Position.X + ((notifWidth - btnWidth) / 2f);
        System.Single btnY = messageBounds.Top + messageBounds.Height + VerticalGapPx + ButtonExtraOffsetY;

        _actionButton.SetPosition(new Vector2f(btnX, btnY));
    }

    /// <summary>
    /// Handles button pressed event: fires external callback and hides this notification.
    /// </summary>
    private void ON_BUTTON_PRESSED()
    {
        OnClicked?.Invoke();
        Hide();
    }

    #endregion
}