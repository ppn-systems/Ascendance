// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Layout;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Desktop.Scenes.Login.View;

/// <summary>
/// Represents the login screen UI with username/password input fields and action buttons.
/// </summary>
internal sealed class LoginView : RenderObject
{
    #region Events

    /// <summary>
    /// Raised when the user requests to submit the login form.
    /// </summary>
    public event System.Action SubmitRequested;

    /// <summary>
    /// Raised when the user requests to navigate back.
    /// </summary>
    public event System.Action BackRequested;

    /// <summary>
    /// Raised when the user toggles between username and password fields using Tab.
    /// </summary>
    /// <remarks>
    /// Parameter is <c>true</c> if tabbing from username to password, <c>false</c> otherwise.
    /// </remarks>
    public event System.Action<System.Boolean> TabToggled;

    #endregion Events

    #region UI Configuration Constants

    private static readonly IntRect SrcRect = default;
    private static readonly Vector2f PanelSize = new(420, 280);
    private static readonly Thickness Border = new(32, 32, 32, 32);

    private static readonly Color WarnColor = Color.Red;
    private static readonly Color TitleColor = Color.White;
    private static readonly Color FieldText = new(30, 30, 30);
    private static readonly Color FieldPanel = new(180, 180, 180);
    private static readonly Color LabelColor = new(240, 240, 240);
    private static readonly Color BgPanelColor = new(20, 20, 20, 235);
    private static readonly Color BackdropColor = new(100, 100, 100, 110);

    // Font sizes (kích thước chữ)
    private const System.Single WarnFont = 14f;           // Giữ warning nhỏ
    private const System.Single TitleFont = 28f;          // Tăng title lên cho nổi bật
    private const System.Single LabelFont = 18f;          // Tăng label lên cho dễ đọc
    private const System.Single FieldFont = 16f;          // Giảm field font xuống cho gọn

    // Field dimensions (kích thước input field)
    private const System.Single FieldWidth = 340f;        // Giữ nguyên
    private const System.Single FieldHeight = 44f;        // Tăng height cho dễ click

    // Layout spacing (khoảng cách các thành phần)
    private const System.Single TitleOffsetX = 40f;       // Lùi title vào trong hơn
    private const System.Single TitleOffsetY = 30f;       // Đẩy title xuống một chút
    private const System.Single LabelUserY = 95f;         // Điều chỉnh vị trí label Username
    private const System.Single LabelPassY = 165f;        // Điều chỉnh vị trí label Password
    private const System.Single FieldLeft = 150f;         // Giữ nguyên
    private const System.Single FieldUserTop = 85f;       // Điều chỉnh field Username xuống
    private const System.Single FieldPassTop = 155f;      // Điều chỉnh field Password xuống

    // Button layout (vị trí nút)
    private const System.Single BtnWidth = 160f;          // Tăng width button lên
    private const System.Single BtnSpacing = 120f;         // Tăng khoảng cách giữa 2 button
    private const System.Single BtnRowOffsetFromBottom = 90f;  // Đẩy buttons lên gần panel hơn

    // Warning message (thông báo lỗi)
    private const System.Single WarnOffsetY = 215f;       // Điều chỉnh warning xuống dưới password

    // Panel scaling (tỷ lệ panel)
    private const System.Single PanelScale = 1.35f;       // Giảm scale xuống một chút cho vừa màn hình

    #endregion UI Configuration Constants

    #region Fields

    private readonly Font _font;
    private readonly Text _warn;
    private readonly Text _title;
    private readonly Text _uLabel;
    private readonly Text _pLabel;
    private readonly Button _backBtn;
    private readonly Button _loginBtn;
    private readonly Texture _texture;
    private readonly Vector2f _panelPos;
    private readonly PasswordField _pass;
    private readonly TextInputField _user;
    private readonly NineSlicePanel _bgPanel;
    private readonly RectangleShape _backdrop;
    private readonly Vector2f _actualPanelSize;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the trimmed username entered by the user.
    /// </summary>
    public System.String Username => _user.Text?.Trim() ?? System.String.Empty;

    /// <summary>
    /// Gets the password entered by the user.
    /// </summary>
    public System.String Password => _pass.Text ?? System.String.Empty;

    /// <summary>
    /// Gets whether the username field currently has focus.
    /// </summary>
    public System.Boolean IsUserFocused => _user.Focused;

    /// <summary>
    /// Gets whether the password field currently has focus.
    /// </summary>
    public System.Boolean IsPassFocused => _pass.Focused;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    public LoginView()
    {
        this.SetZIndex(2);
        this.IsEnabled = true;

        _font = EmbeddedAssets.JetBrainsMono.ToFont();
        _texture = EmbeddedAssets.SquareOutline.ToTexture();

        _actualPanelSize = PanelSize * PanelScale;
        _panelPos = GET_CENTERED_POSITION(_actualPanelSize);

        // Initialize backdrop (full screen overlay)
        _backdrop = new RectangleShape((Vector2f)GraphicsEngine.ScreenSize)
        {
            FillColor = BackdropColor
        };

        // Initialize background panel
        _bgPanel = new NineSlicePanel(_texture, Border, SrcRect).SetSize(_actualPanelSize)
                                                                .SetPosition(_panelPos)
                                                                .SetTintColor(BgPanelColor);

        // Initialize text labels
        _title = new Text("LOGIN", _font, (System.UInt32)TitleFont)
        {
            FillColor = TitleColor
        };

        _uLabel = new Text("Username", _font, (System.UInt32)LabelFont)
        {
            FillColor = LabelColor
        };

        _pLabel = new Text("Password", _font, (System.UInt32)LabelFont)
        {
            FillColor = LabelColor
        };

        _warn = new Text(System.String.Empty, _font, (System.UInt32)WarnFont)
        {
            FillColor = WarnColor
        };

        // Initialize username input field
        _user = new TextInputField(
            _texture,
            Border,
            SrcRect,
            new Vector2f(FieldWidth, FieldHeight),
            new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldUserTop),
            _font,
            (System.UInt32)FieldFont)
        {
            PanelColor = FieldPanel,
            TextColor = FieldText,
            Placeholder = "Enter username"
        };

        // Initialize password input field
        _pass = new PasswordField(
            _texture,
            Border,
            SrcRect,
            new Vector2f(FieldWidth, FieldHeight),
            new Vector2f(_panelPos.X + FieldLeft, _panelPos.Y + FieldPassTop),
            _font,
            (System.UInt32)FieldFont)
        {
            PanelColor = FieldPanel,
            TextColor = FieldText,
            Placeholder = "Enter password"
        };

        // Initialize buttons
        _loginBtn = new Button("Login", null, BtnWidth);
        _loginBtn.SetZIndex(2);
        _loginBtn.RegisterClickHandler(this.ON_LOGIN_CLICKED);

        _backBtn = new Button("Back", null, BtnWidth);
        _backBtn.SetZIndex(2);
        _backBtn.RegisterClickHandler(this.ON_BACK_CLICKED);

        this.LAYOUT();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Locks or unlocks the UI for interaction during login processing.
    /// </summary>
    /// <param name="locked">
    /// <c>true</c> to disable all interactive elements; <c>false</c> to enable them.
    /// </param>
    public void LockUi(System.Boolean locked)
    {
        _user.IsEnabled = !locked;
        _pass.IsEnabled = !locked;
        _backBtn.IsEnabled = !locked;
        _loginBtn.IsEnabled = !locked;
        _loginBtn.Text = locked ? "Signing in..." : "Sign in";
    }

    /// <summary>
    /// Displays a warning message below the password field.
    /// </summary>
    /// <param name="msg">The warning message to display.</param>
    public void ShowWarning(System.String msg) => _warn.DisplayedString = msg ?? System.String.Empty;

    /// <summary>
    /// Clears the warning message.
    /// </summary>
    public void ClearWarning() => _warn.DisplayedString = System.String.Empty;

    #endregion Public Methods

    #region Keyboard Event Handlers

    /// <summary>
    /// Handles the Enter key press: moves focus from username to password,
    /// or submits the form if password field is focused.
    /// </summary>
    public void OnEnter()
    {
        if (this.IsUserFocused)
        {
            _pass.OnFocusGained();
            _user.OnFocusLost();
        }
        else if (this.IsPassFocused)
        {
            this.SubmitRequested?.Invoke();
        }
    }

    /// <summary>
    /// Handles the Escape key press: triggers back navigation.
    /// </summary>
    public void OnEscape() => this.BackRequested?.Invoke();

    /// <summary>
    /// Handles the Tab key press: toggles focus between username and password fields.
    /// </summary>
    public void OnTab()
    {
        System.Boolean movingToPassword = _user.Focused;

        if (movingToPassword)
        {
            _pass.OnFocusGained();
            _user.OnFocusLost();
        }
        else
        {
            _user.OnFocusGained();
            _pass.OnFocusLost();
        }

        this.TabToggled?.Invoke(movingToPassword);
    }

    /// <summary>
    /// Toggles the visibility of the password text (show/hide).
    /// </summary>
    public void OnTogglePassword() => _pass.ToggleVisibility();

    #endregion Keyboard Event Handlers

    #region Overrides

    /// <summary>
    /// Updates all interactive UI elements.
    /// </summary>
    /// <param name="dt">Delta time in seconds since last frame.</param>
    public override void Update(System.Single dt)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.HANDLE_KEYBOARD_INPUT();

        _user.Update(dt);
        _pass.Update(dt);
        _backBtn.Update(dt);
        _loginBtn.Update(dt);
    }

    /// <summary>
    /// Renders all UI elements to the target.
    /// </summary>
    /// <param name="target">The render target.</param>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_backdrop);

        _bgPanel.Draw(target);
        _user.Draw(target);
        _pass.Draw(target);
        _backBtn.Draw(target);
        _loginBtn.Draw(target);

        target.Draw(_title);
        target.Draw(_uLabel);
        target.Draw(_pLabel);
        target.Draw(_warn);
    }

    /// <summary>
    /// Returns the backdrop drawable (required by base class).
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => _backdrop;

    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// Positions all UI elements based on the panel position and size.
    /// </summary>
    private void LAYOUT()
    {
        // Position title
        _title.Position = new Vector2f(
            _panelPos.X + TitleOffsetX,
            _panelPos.Y + TitleOffsetY);

        // Position labels
        _uLabel.Position = new Vector2f(
            _panelPos.X + TitleOffsetX,
            _panelPos.Y + LabelUserY);

        _pLabel.Position = new Vector2f(
            _panelPos.X + TitleOffsetX,
            _panelPos.Y + LabelPassY);

        // Position warning text
        _warn.Position = new Vector2f(
            _panelPos.X + TitleOffsetX,
            _panelPos.Y + WarnOffsetY);

        // Calculate button row position (centered horizontally, near bottom)
        System.Single totalBtnWidth = (BtnWidth * 2f) + BtnSpacing;
        System.Single btnStartX = _panelPos.X + ((_actualPanelSize.X - totalBtnWidth) * 0.5f);
        System.Single btnY = _panelPos.Y + _actualPanelSize.Y - BtnRowOffsetFromBottom;

        _backBtn.Position = new Vector2f(btnStartX, btnY);
        _loginBtn.Position = new Vector2f(btnStartX + BtnWidth + BtnSpacing, btnY);
    }

    /// <summary>
    /// Calculates the centered position for a given size on the screen.
    /// </summary>
    /// <param name="size">The size of the element to center.</param>
    /// <returns>The top-left position that centers the element.</returns>
    private static Vector2f GET_CENTERED_POSITION(Vector2f size)
    {
        return new Vector2f(
            (GraphicsEngine.ScreenSize.X - size.X) * 0.5f,
            (GraphicsEngine.ScreenSize.Y - size.Y) * 0.5f);
    }

    /// <summary>
    /// Handles the login button click event.
    /// </summary>
    private void ON_LOGIN_CLICKED() => this.SubmitRequested?.Invoke();

    /// <summary>
    /// Handles the back button click event.
    /// </summary>
    private void ON_BACK_CLICKED() => this.BackRequested?.Invoke();

    /// <summary>
    /// Handles keyboard input for Tab, Enter, and Escape keys.
    /// </summary>
    private void HANDLE_KEYBOARD_INPUT()
    {
        // Tab: Toggle focus between fields
        if (KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Tab))
        {
            this.OnTab();
        }

        // Enter: Move to next field or submit
        if (KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter))
        {
            this.OnEnter();
        }

        // Escape: Go back
        if (KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Escape))
        {
            this.OnEscape();
        }

        // Optional: Ctrl+H to toggle password visibility
        System.Boolean ctrlPressed = KeyboardManager.Instance.IsKeyDown(Keyboard.Key.LControl)
                                   || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.RControl);

        if (ctrlPressed && KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.H))
        {
            this.OnTogglePassword();
        }
    }

    #endregion Private Methods
}