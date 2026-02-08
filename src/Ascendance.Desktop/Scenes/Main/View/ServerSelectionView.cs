// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Layout;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Main.View;

/// <summary>
/// Represents the server selection screen where players choose which game server to join.
/// </summary>
internal sealed class ServerSelectionView : RenderObject
{
    #region Events

    /// <summary>
    /// Raised when a server is selected.
    /// </summary>
    public event System.Action<System.Int32> ServerSelected;

    /// <summary>
    /// Raised when the language dropdown is clicked.
    /// </summary>
    public event System.Action LanguageDropdownClicked;

    /// <summary>
    /// Raised when "Clear Data" button is clicked.
    /// </summary>
    public event System.Action BackRequested;

    /// <summary>
    /// Raised when server type tab is changed (Standard/Super).
    /// </summary>
    public event System.Action<ServerType> ServerTypeChanged;

    #endregion Events

    #region Constants

    // Colors
    private static readonly Color TitleTextColor = new(100, 60, 20);
    private static readonly Color ActiveTabColor = new(180, 220, 140); // Light green
    private static readonly Color InactiveTabColor = new(245, 220, 180); // Cream
    private static readonly Color TitleBgColor = new(245, 220, 180, 255);
    private static readonly Color PanelBgColor = new(210, 180, 140, 240); // Tan/brown
    private static readonly Color BackgroundTint = new(255, 255, 255, 255);

    // Layout
    private const System.Single TitleWidth = 260f;
    private const System.Single TitleHeight = 60f;

    private const System.Single MainPanelWidth = 750f;
    private const System.Single MainPanelHeight = 410f;

    private const System.Single LeftPanelWidth = 240f;
    private const System.Single TabHeight = 50f;
    private const System.Single TabSpacing = 25f;

    private const System.Single ServerBtnWidth = 100f;
    private const System.Single ServerBtnHeight = 40f;
    private const System.Single ServerBtnSpacingX = 150f;
    private const System.Single ServerBtnSpacingY = 22f;

    private static readonly Thickness PanelBorder = new(32);

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Text _titleText;
    private readonly Button _backBtn;
    private readonly Sprite _background;
    private readonly Button _superTabBtn;
    private readonly Vector2f _centerPos;
    private readonly Text _descriptionText;
    private readonly Texture _panelTexture;
    private readonly Button _standardTabBtn;
    private readonly Button[] _serverButtons;
    private readonly NineSlicePanel _mainPanel;
    private readonly NineSlicePanel _titlePanel;
    private readonly NineSlicePanel _descriptionPanel;

    private ServerType _currentServerType = ServerType.Standard;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectionView"/> class.
    /// </summary>
    public ServerSelectionView()
    {
        this.SetZIndex(1);
        this.IsEnabled = true;

        _font = EmbeddedAssets.JetBrainsMono.ToFont();
        _panelTexture = EmbeddedAssets.SquareOutline.ToTexture();
        _centerPos = new Vector2f(GraphicsEngine.ScreenSize.X * 0.5f, GraphicsEngine.ScreenSize.Y * 0.5f);

        // Background
        Texture bgTexture = AssetManager.Instance.LoadTexture("res/texture/wcp/0");
        _background = new Sprite(bgTexture)
        {
            Position = new Vector2f(0, 0),
            Color = BackgroundTint
        };

        // Scale background to fit screen
        System.Single scale = System.Math.Max(
            (System.Single)GraphicsEngine.ScreenSize.X / bgTexture.Size.X,
            (System.Single)GraphicsEngine.ScreenSize.Y / bgTexture.Size.Y
        );
        _background.Scale = new Vector2f(scale, scale);

        // Title panel
        Vector2f titlePos = new(_centerPos.X - (TitleWidth * 0.5f), 100f);
        _titlePanel = new NineSlicePanel(_panelTexture, PanelBorder, default)
            .SetPosition(titlePos)
            .SetSize(new Vector2f(TitleWidth, TitleHeight));
        _titlePanel.SetTintColor(TitleBgColor);

        _titleText = new Text("Chọn máy chủ", _font, 24)
        {
            FillColor = TitleTextColor,
            Position = new Vector2f(
                titlePos.X + (TitleWidth * 0.5f) - 80f,
                titlePos.Y + 15f)
        };

        // Main panel
        Vector2f mainPanelPos = new(
            _centerPos.X - (MainPanelWidth * 0.5f),
            titlePos.Y + TitleHeight + 20f);

        _mainPanel = new NineSlicePanel(_panelTexture, PanelBorder, default)
            .SetPosition(mainPanelPos)
            .SetSize(new Vector2f(MainPanelWidth, MainPanelHeight));
        _mainPanel.SetTintColor(PanelBgColor);

        // Left panel tabs
        System.Single tabX = mainPanelPos.X + 20f;
        System.Single tabY = mainPanelPos.Y + 20f;

        _standardTabBtn = new Button("Máy chủ tiêu chuẩn", null, LeftPanelWidth - 20f)
        {
            Position = new Vector2f(tabX, tabY),
            Height = TabHeight,
            TextColor = Color.Black
        };
        _standardTabBtn.RegisterClickHandler(() => this.SWITCH_TAB(ServerType.Standard));

        _superTabBtn = new Button("Máy chủ Super", null, LeftPanelWidth - 20f)
        {
            Position = new Vector2f(tabX, tabY + TabHeight + TabSpacing),
            Height = TabHeight,
            TextColor = Color.Black
        };
        _superTabBtn.RegisterClickHandler(() => this.SWITCH_TAB(ServerType.Super));

        // Description panel
        System.Single descY = tabY + ((TabHeight + TabSpacing) * 2) + 15f;
        _descriptionPanel = new NineSlicePanel(_panelTexture, new Thickness(16), default)
            .SetPosition(new Vector2f(tabX, descY))
            .SetSize(new Vector2f(LeftPanelWidth - 20f, 150f));
        _descriptionPanel.SetTintColor(new Color(160, 130, 100, 200));

        _descriptionText = new Text(
            "Máy chủ tiêu chuẩn:\nTiến trình game bình\nthường.",
            _font, 14)
        {
            FillColor = Color.White,
            Position = new Vector2f(tabX + 10f, descY + 10f)
        };

        // Server buttons (2 columns x 6 rows)
        _serverButtons = new Button[2];
        System.Single serverBtnStartX = mainPanelPos.X + LeftPanelWidth + 30f;
        System.Single serverBtnStartY = mainPanelPos.Y + 20f;

        for (System.Int32 i = 0; i < 2; i++)
        {
            System.Int32 col = i % 2;
            System.Int32 row = i / 2;

            System.Single btnX = serverBtnStartX + (col * (ServerBtnWidth + ServerBtnSpacingX));
            System.Single btnY = serverBtnStartY + (row * (ServerBtnHeight + ServerBtnSpacingY));

            System.Int32 serverNum = i + 1;
            _serverButtons[i] = new Button($"Vũ trụ {serverNum}", null, ServerBtnWidth)
            {
                Position = new Vector2f(btnX, btnY),
                Height = ServerBtnHeight
            };

            // Capture serverNum in closure
            System.Int32 capturedNum = serverNum;
            _serverButtons[i].RegisterClickHandler(() => this.ON_SERVER_SELECTED(capturedNum));
        }

        // Clear data button (bottom-right)
        _backBtn = new Button("Back", null, 150f)
        {
            Position = new Vector2f(
                GraphicsEngine.ScreenSize.X - 220f,
                GraphicsEngine.ScreenSize.Y - 90f)
        };
        _backBtn.RegisterClickHandler(this.ON_CLEAR_DATA_CLICKED);

        // Set initial tab state
        this.UPDATE_TAB_VISUALS();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Sets which servers are available/playable.
    /// </summary>
    /// <param name="playedServers">Array of server IDs that have been played (1-12).</param>
    public void SetPlayedServers(System.Int32[] playedServers)
    {
        System.Collections.Generic.HashSet<System.Int32> playedSet =
            [.. playedServers];

        for (System.Int32 i = 0; i < _serverButtons.Length; i++)
        {
            System.Int32 serverId = i + 1;
            _serverButtons[i].IsEnabled = playedSet.Contains(serverId);
        }
    }

    #endregion Public Methods

    #region Overrides

    /// <summary>
    /// Updates all interactive UI elements.
    /// </summary>
    public override void Update(System.Single dt)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _standardTabBtn.Update(dt);
        _superTabBtn.Update(dt);
        _backBtn.Update(dt);

        foreach (Button btn in _serverButtons)
        {
            btn.Update(dt);
        }
    }

    /// <summary>
    /// Renders all UI elements.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);

        _titlePanel.Draw(target);
        target.Draw(_titleText);

        _mainPanel.Draw(target);

        _standardTabBtn.Draw(target);
        _superTabBtn.Draw(target);

        _descriptionPanel.Draw(target);
        target.Draw(_descriptionText);

        foreach (Button btn in _serverButtons)
        {
            btn.Draw(target);
        }

        _backBtn.Draw(target);
    }

    /// <summary>
    /// Returns the background sprite.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => _background;

    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// Handles tab switching between Standard and Super servers.
    /// </summary>
    private void SWITCH_TAB(ServerType serverType)
    {
        if (_currentServerType == serverType)
        {
            return;
        }

        _currentServerType = serverType;
        this.UPDATE_TAB_VISUALS();
        this.ServerTypeChanged?.Invoke(serverType);
    }

    /// <summary>
    /// Updates tab button colors and description text based on selected tab.
    /// </summary>
    private void UPDATE_TAB_VISUALS()
    {
        if (_currentServerType == ServerType.Standard)
        {
            _standardTabBtn.PanelColor = ActiveTabColor;
            _superTabBtn.PanelColor = InactiveTabColor;
            _descriptionText.DisplayedString = "Máy chủ tiêu chuẩn:\nTiến trình game bình\nthường.";
        }
        else
        {
            _standardTabBtn.PanelColor = InactiveTabColor;
            _superTabBtn.PanelColor = ActiveTabColor;
            _descriptionText.DisplayedString = "Máy chủ Super:\nTốc độ game nhanh\nhơn, phần thưởng cao.";
        }
    }

    /// <summary>
    /// Handles server button click.
    /// </summary>
    private void ON_SERVER_SELECTED(System.Int32 serverId) => this.ServerSelected?.Invoke(serverId);

    /// <summary>
    /// Handles language dropdown click.
    /// </summary>
    private void ON_LANGUAGE_CLICKED() => this.LanguageDropdownClicked?.Invoke();

    /// <summary>
    /// Handles clear data button click.
    /// </summary>
    private void ON_CLEAR_DATA_CLICKED() => this.BackRequested?.Invoke();

    #endregion Private Methods
}

/// <summary>
/// Represents the type of game server.
/// </summary>
public enum ServerType
{
    /// <summary>
    /// Standard server with normal game progression.
    /// </summary>
    Standard,

    /// <summary>
    /// Super server with accelerated progression and enhanced rewards.
    /// </summary>
    Super
}