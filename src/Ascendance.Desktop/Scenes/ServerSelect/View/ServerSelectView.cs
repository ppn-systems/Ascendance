// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Layout;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
using Ascendance.Shared.Enums;
using Nalix.Framework.Configuration;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.ServerSelect.View;

/// <summary>
/// Represents the server selection screen where players choose which game server to join.
/// </summary>
internal sealed class ServerSelectView : RenderObject
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
    /// Raised when "Back" button is clicked.
    /// </summary>
    public event System.Action BackRequested;

    /// <summary>
    /// Raised when server type tab is changed (Domestic/Global).
    /// </summary>
    public event System.Action<ServerType> ServerTypeChanged;

    #endregion Events

    #region Constants - Improved Color Palette

    // Main Panel - Dark wood theme
    private static readonly Color MainPanelBg = new(70, 55, 45, 240);

    // Tab Buttons - Enhanced green/cream contrast
    private static readonly Color ActiveTabBg = new(130, 190, 110, 255); // Vibrant green
    private static readonly Color ActiveTabBorder = new(85, 140, 70, 255);
    private static readonly Color ActiveTabText = new(25, 45, 20);

    private static readonly Color InactiveTabBg = new(210, 185, 150, 240); // Warm cream
    private static readonly Color InactiveTabBorder = new(150, 120, 90, 255);
    private static readonly Color InactiveTabText = new(70, 55, 40);

    // Description Panel - Parchment style
    private static readonly Color DescPanelBg = new(180, 150, 110, 210);
    private static readonly Color DescTextColor = new(240, 230, 210);

    // Server Buttons - Dark with bronze accents
    private static readonly Color ServerBtnNormal = new(50, 45, 42, 245);
    private static readonly Color ServerBtnNormalText = new(200, 180, 150);

    private static readonly Color ServerBtnDisabled = new(60, 55, 50, 180);
    private static readonly Color ServerBtnDisabledText = new(100, 90, 80);

    // Back Button
    private static readonly Color BackBtnBg = new(90, 70, 60, 230);
    private static readonly Color BackBtnText = new(220, 200, 170);

    // Layout
    private const System.Single TitleHeight = 65f;
    private const System.Single MainPanelWidth = 750f;
    private const System.Single MainPanelHeight = 410f;
    private const System.Single LeftPanelWidth = 250f;
    private const System.Single TabHeight = 52f;
    private const System.Single TabSpacing = 18f;
    private const System.Single ServerBtnWidth = 220f;
    private const System.Single ServerBtnHeight = 55f;
    private const System.Single ServerBtnSpacingX = -5f;
    private const System.Single ServerBtnSpacingY = 25f;

    private static readonly Thickness DescBorder = new(16);
    private static readonly Thickness PanelBorder = new(32);

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Button _backBtn;
    private readonly Button _globalTabBtn;
    private readonly Vector2f _centerPos;
    private readonly Text _descriptionText;
    private readonly Texture _panelTexture;
    private readonly Button _domesticTabBtn;
    private readonly Button[] _serverButtons;
    private readonly NineSlicePanel _mainPanel;
    private readonly NineSlicePanel _descriptionPanel;

    private ServerType _currentServerType = ServerType.Domestic;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectView"/> class.
    /// </summary>
    public ServerSelectView()
    {
        this.SetZIndex(1);
        this.IsEnabled = true;

        _font = EmbeddedAssets.JetBrainsMono.ToFont();
        _panelTexture = EmbeddedAssets.SquareOutline.ToTexture();
        _centerPos = new Vector2f(GraphicsEngine.ScreenSize.X * 0.5f, GraphicsEngine.ScreenSize.Y * 0.5f);

        // Main panel - Dark wood
        Vector2f mainPanelPos = new(_centerPos.X - (MainPanelWidth * 0.5f), TitleHeight * 2.5f);

        _mainPanel = new NineSlicePanel(_panelTexture, PanelBorder, default)
            .SetPosition(mainPanelPos)
            .SetSize(new Vector2f(MainPanelWidth, MainPanelHeight))
            .SetTintColor(MainPanelBg);

        // Tab buttons
        System.Single tabX = mainPanelPos.X + 25f;
        System.Single tabY = mainPanelPos.Y + 25f;

        _domesticTabBtn = new Button("DOMESTIC SERVER", null, LeftPanelWidth - 30f)
        {
            Position = new Vector2f(tabX, tabY),
            Height = TabHeight,
            FontSize = 16
        };
        _domesticTabBtn.RegisterClickHandler(() => this.SWITCH_TAB(ServerType.Domestic));

        _globalTabBtn = new Button("GLOBAL SERVER", null, LeftPanelWidth - 30f)
        {
            Position = new Vector2f(tabX, tabY + TabHeight + TabSpacing),
            Height = TabHeight,
            FontSize = 16
        };
        _globalTabBtn.RegisterClickHandler(() => this.SWITCH_TAB(ServerType.Global));

        // Description panel - Parchment style
        System.Single descY = tabY + ((TabHeight + TabSpacing) * 2) + 20f;
        _descriptionPanel = new NineSlicePanel(_panelTexture, DescBorder, default)
            .SetPosition(new Vector2f(tabX, descY))
            .SetSize(new Vector2f(LeftPanelWidth - 30f, 170f))
            .SetTintColor(DescPanelBg);

        _descriptionText = new Text("", _font, 15)
        {
            LineSpacing = 1.4f,
            OutlineThickness = 1f,
            FillColor = DescTextColor,
            OutlineColor = new Color(80, 60, 40),
            Position = new Vector2f(tabX + 15f, descY + 15f)
        };

        // Server buttons (2 columns)
        _serverButtons = new Button[2];
        System.Single serverBtnStartX = mainPanelPos.X + LeftPanelWidth + 45f;
        System.Single serverBtnStartY = mainPanelPos.Y + 30f;

        for (System.Int32 i = 0; i < 2; i++)
        {
            System.Int32 col = i % 2;
            System.Int32 row = i / 2;

            System.Single btnX = serverBtnStartX + (col * (ServerBtnWidth + ServerBtnSpacingX));
            System.Single btnY = serverBtnStartY + (row * (ServerBtnHeight + ServerBtnSpacingY));

            System.Int32 serverNum = i + 1;
            _serverButtons[i] = new Button($"Server {serverNum}", null, ServerBtnWidth)
            {
                FontSize = 20,
                Height = ServerBtnHeight,
                Position = new Vector2f(btnX, btnY)
            };

            // Apply custom colors
            APPLY_SERVER_BUTTON_STYLE(_serverButtons[i]);

            System.Int32 capturedNum = serverNum;

            _serverButtons[i].RegisterClickHandler(BACK_MAIN);
            _serverButtons[i].RegisterClickHandler(() => this.ON_SERVER_SELECTED(capturedNum));
        }

        // Back button
        _backBtn = new Button("Back", null, 160f)
        {
            Height = 50f,
            FontSize = 18,
            PanelColor = BackBtnBg,
            TextColor = BackBtnText,
            Position = new Vector2f(GraphicsEngine.ScreenSize.X - 210f, GraphicsEngine.ScreenSize.Y - 80f)
        };
        _backBtn.RegisterClickHandler(this.ON_BACK_CLICKED);

        // Set initial tab state
        this.UPDATE_TAB_VISUALS();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Sets which servers are available/playable.
    /// </summary>
    /// <param name="playedServers">Array of server IDs that have been played.</param>
    public void SetPlayedServers(System.Int32[] playedServers)
    {
        System.Collections.Generic.HashSet<System.Int32> playedSet = [.. playedServers];

        for (System.Int32 i = 0; i < _serverButtons.Length; i++)
        {
            System.Int32 serverId = i + 1;
            System.Boolean isEnabled = playedSet.Contains(serverId);

            _serverButtons[i].IsEnabled = isEnabled;

            if (!isEnabled)
            {
                _serverButtons[i].PanelColor = ServerBtnDisabled;
                _serverButtons[i].TextColor = ServerBtnDisabledText;
            }
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

        _domesticTabBtn.Update(dt);
        _globalTabBtn.Update(dt);
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

        // Draw main panel
        _mainPanel.Draw(target);

        // Draw tabs
        _domesticTabBtn.Draw(target);
        _globalTabBtn.Draw(target);

        // Draw description
        _descriptionPanel.Draw(target);
        target.Draw(_descriptionText);

        // Draw server buttons
        if (_currentServerType == ServerType.Domestic)
        {
            // For Domestic, show both servers
            foreach (Button btn in _serverButtons)
            {
                btn.Draw(target);
            }
        }
        else
        {
            // For Global, only show the second server
        }

        // Draw back button
        _backBtn.Draw(target);
    }

    /// <summary>
    /// Returns the background sprite.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => throw new System.NotImplementedException();

    #endregion Overrides

    #region Private Methods

    private static void BACK_MAIN() => SceneManager.Instance.ScheduleSceneChange(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

    /// <summary>
    /// Applies custom styling to server button.
    /// </summary>
    private static void APPLY_SERVER_BUTTON_STYLE(Button button)
    {
        button.PanelColor = ServerBtnNormal;
        button.TextColor = ServerBtnNormalText;
        button.TextOutlineThickness = 2f;
        button.TextOutlineColor = new Color(25, 20, 18);
    }

    /// <summary>
    /// Handles tab switching between Domestic and Global servers.
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
        if (_currentServerType == ServerType.Domestic)
        {
            // Active: Domestic
            _domesticTabBtn.PanelColor = ActiveTabBg;
            _domesticTabBtn.TextColor = ActiveTabText;
            _domesticTabBtn.TextOutlineThickness = 2.5f;
            _domesticTabBtn.TextOutlineColor = ActiveTabBorder;

            // Inactive: Global
            _globalTabBtn.PanelColor = InactiveTabBg;
            _globalTabBtn.TextColor = InactiveTabText;
            _globalTabBtn.TextOutlineThickness = 1f;
            _globalTabBtn.TextOutlineColor = InactiveTabBorder;

            _descriptionText.DisplayedString =
                "Máy chủ tiêu chuẩn:\n\n" +
                "Tiến trình game bình\n" +
                "thường, phù hợp cho\n" +
                "người chơi mới.";
        }
        else
        {
            // Active: Global
            _globalTabBtn.PanelColor = ActiveTabBg;
            _globalTabBtn.TextColor = ActiveTabText;
            _globalTabBtn.TextOutlineThickness = 2.5f;
            _globalTabBtn.TextOutlineColor = ActiveTabBorder;

            // Inactive: Domestic
            _domesticTabBtn.PanelColor = InactiveTabBg;
            _domesticTabBtn.TextColor = InactiveTabText;
            _domesticTabBtn.TextOutlineThickness = 1f;
            _domesticTabBtn.TextOutlineColor = InactiveTabBorder;

            _descriptionText.DisplayedString =
                "Máy chủ quốc tế:\n\n" +
                "Kết nối với người chơi\n" +
                "toàn thế giới, nhiều\n" +
                "thử thách hơn.";
        }
    }

    /// <summary>
    /// Handles server button click.
    /// </summary>
    private void ON_SERVER_SELECTED(System.Int32 serverId) => this.ServerSelected?.Invoke(serverId);

    /// <summary>
    /// Handles back button click.
    /// </summary>
    private void ON_BACK_CLICKED() => this.BackRequested?.Invoke();

    #endregion Private Methods
}