// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Main.View;

/// <summary>
/// Represents a view containing the main menu buttons for the application.
/// Manages the layout, rendering, and interaction of login, new game, server info, and change account buttons.
/// </summary>
public class ButtonView : RenderObject
{
    #region Const

    private const System.Single ButtonWidth = 380f;
    private const System.Single VerticalSpacing = 20f;
    private const System.Single HorizontalCenterDivisor = 2f;
    private const System.Single VerticalCenterDivisor = 1.65f;

    #endregion Const

    #region Fields

    private readonly Button _login;
    private readonly Button _newGame;
    private readonly Button _serverInfo;
    private readonly Button _changeAccount;

    private readonly Button[] _buttons;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when the login button is clicked.
    /// </summary>
    public event System.Action LoginRequested;

    /// <summary>
    /// Raised when the new game button is clicked.
    /// </summary>
    public event System.Action NewGameRequested;

    /// <summary>
    /// Raised when the server info button is clicked.
    /// </summary>
    public event System.Action ServerInfoRequested;

    /// <summary>
    /// Raised when the change account button is clicked.
    /// </summary>
    public event System.Action ChangeAccountRequested;

    #endregion Events

    #region Properties

    /// <summary>
    /// Login button visibility property.
    /// </summary>
    public System.Boolean IsLoginButtonVisible
    {
        get => _login.IsVisible;
        set
        {
            if (value)
            {
                _login.Show();
            }
            else if (!value)
            {
                _login.Hide();
            }

            LAYOUT_BUTTONS();
        }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonView"/> class.
    /// Creates and configures all buttons, wires event handlers, and sets up the layout.
    /// </summary>
    public ButtonView()
    {
        _login = new Button("Login");
        _newGame = new Button("New game");
        _serverInfo = new Button("Server");
        _changeAccount = new Button("Change account");

        _buttons = [_login, _newGame, _changeAccount, _serverInfo];

        this.WIRE_HANDLERS();
        this.REGISTER_BUTTONS();
        this.LAYOUT_BUTTONS();
    }

    #endregion Constructor

    #region Overrides

    /// <inheritdoc/>
    public override void Update(System.Single dt)
    {
        if (!base.IsVisible)
        {
            return;
        }

        foreach (Button b in _buttons)
        {
            b.Update(dt);
        }
    }

    /// <inheritdoc/>
    public override void Draw(RenderTarget target)
    {
        foreach (Button b in _buttons)
        {
            b.Draw(target);
        }
    }

    /// <inheritdoc/>
    protected override Drawable GetDrawable() => throw new System.NotSupportedException();

    #endregion Overrides

    #region Private Methods

    private void REGISTER_BUTTONS()
    {
        foreach (Button b in _buttons)
        {
            b.Size = new Vector2f(ButtonWidth, b.Size.Y);
            b.FontSize = 17;
        }
    }

    private void WIRE_HANDLERS()
    {
        _login.RegisterClickHandler(() => LoginRequested?.Invoke());
        _newGame.RegisterClickHandler(() => NewGameRequested?.Invoke());
        _serverInfo.RegisterClickHandler(() => ServerInfoRequested?.Invoke());
        _changeAccount.RegisterClickHandler(() => ChangeAccountRequested?.Invoke());
    }

    private void LAYOUT_BUTTONS()
    {
        System.Single total = 0f;
        foreach (Button b in _buttons)
        {
            if (!b.IsVisible)
            {
                continue;
            }
            total += b.GlobalBounds.Height + VerticalSpacing;
        }

        total -= VerticalSpacing;

        System.Single y = (GraphicsEngine.ScreenSize.Y - total) / VerticalCenterDivisor;

        foreach (Button b in _buttons)
        {
            if (!b.IsVisible)
            {
                continue;
            }
            FloatRect r = b.GlobalBounds;
            System.Single x = (GraphicsEngine.ScreenSize.X - r.Width) / HorizontalCenterDivisor;

            b.Position = new Vector2f(x, y);
            y += r.Height + VerticalSpacing;
        }
    }

    #endregion Private Methods
}