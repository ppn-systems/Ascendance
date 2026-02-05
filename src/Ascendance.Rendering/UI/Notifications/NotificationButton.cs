// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Notifications;

/// <summary>
/// Notification box with a single action button using reusable Button class.
/// Supports click visual effects and automatically closes when the button is clicked.
/// </summary>
public sealed class NotificationButton : Notification
{
    #region Constants

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
    private const System.Single DefaultButtonWidth = 180f;

    /// <summary>
    /// Default button height.
    /// </summary>
    private const System.Single DefaultButtonHeight = 32f;

    #endregion Constants

    #region Fields

    /// <summary>
    /// The action button (reused from UI.Controls.Button).
    /// </summary>
    private readonly Button _actionButton;

    /// <summary>
    /// Button extra vertical offset after layout.
    /// </summary>
    private System.Single _buttonExtraOffsetY = 35;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Callback fired when button is clicked.
    /// </summary>
    private event System.Action OnClicked;

    /// <summary>
    /// Extra vertical offset for the button after layout.
    /// </summary>
    public System.Single ButtonExtraOffsetY
    {
        get => _buttonExtraOffsetY;
        set
        {
            _buttonExtraOffsetY = value;
            this.UPDATE_BUTTON_LAYOUT();
        }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a notification box with an action button under the message.
    /// </summary>
    /// <param name="buttonTexture">Texture for button panel.</param>
    /// <param name="initialMessage">Initial notification message.</param>
    /// <param name="side">Side of the screen to display notification.</param>
    /// <param name="buttonText">Label of action button.</param>
    /// <param name="font">Font for notification and button text.</param>
    public NotificationButton(
        Texture buttonTexture = null, System.String initialMessage = "",
        Direction2D side = Direction2D.Down, System.String buttonText = "OK", Font font = null)
        : base(buttonTexture, initialMessage, side, font)
    {
        font ??= EmbeddedAssets.JetBrainsMono.ToFont();
        buttonTexture ??= EmbeddedAssets.SquareOutline.ToTexture();

        // Tạo button, có thể chỉnh pad, màu tuỳ ý qua các hàm của Button.
        _actionButton = new Button(buttonText, buttonTexture, 240, default, font)
            .SetFontSize(ButtonFontSize)
            .SetSize(DefaultButtonWidth, DefaultButtonHeight);

        // Đăng ký sự kiện click để đóng notification và gọi OnClicked.
        _actionButton.RegisterClickHandler(this.ON_BUTTON_PRESSED);

        // Lần đầu layout nút.
        this.UPDATE_BUTTON_LAYOUT();
        base.SetZIndex(RenderLayer.NotificationButton.ToZIndex());
    }

    #endregion Constructor

    #region Event Registration

    /// <summary>
    /// Registers a callback to be invoked when the button is clicked.
    /// </summary>
    public void RegisterAction(System.Action handler) => this.OnClicked += handler;

    /// <summary>
    /// Unregisters a previously registered action callback.
    /// </summary>
    public void UnregisterAction(System.Action handler) => this.OnClicked -= handler;

    #endregion Event Registration

    #region Overrides

    /// <summary>
    /// Sets Z-Index of notification and its button.
    /// </summary>
    public new void SetZIndex(System.Int32 zOrder)
    {
        base.SetZIndex(zOrder);
        _actionButton.SetZIndex(zOrder + 1);
    }

    /// <summary>
    /// Updates notification and its button.
    /// </summary>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible)
        {
            return;
        }

        base.Update(deltaTime);
        _actionButton.Update(deltaTime);
    }

    /// <summary>
    /// Renders notification base and action button.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
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
        this.UPDATE_BUTTON_LAYOUT();
    }

    #endregion Overrides

    #region Private Logic

    /// <summary>
    /// Lays out the action button underneath the notification message.
    /// </summary>
    private void UPDATE_BUTTON_LAYOUT()
    {
        FloatRect messageBounds = MessageText.GetGlobalBounds();

        // Trung tâm theo chiều ngang notification, bên dưới message một đoạn.
        System.Single x = Panel.Position.X + ((Panel.Size.X - DefaultButtonWidth) / 2f);
        System.Single y = messageBounds.Top + messageBounds.Height + VerticalGapPx + ButtonExtraOffsetY;

        _actionButton.SetPosition(new Vector2f(x, y));
    }

    /// <summary>
    /// Handles button pressed event: fires external callback and hides this notification.
    /// </summary>
    private void ON_BUTTON_PRESSED()
    {
        this.OnClicked?.Invoke();
        base.Hide();
    }

    #endregion Private Logic
}