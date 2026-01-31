// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.UI.Theme;

/// <summary>
/// Provides predefined color themes for buttons.
/// </summary>
public static class Themes
{
    /// <summary>
    /// Default panel color theme for buttons (Normal, Hover, Disabled).
    /// </summary>
    public static readonly ButtonStateColors PanelTheme = new(
        new Color(30, 30, 30),
        new Color(60, 60, 60),
        new Color(40, 40, 40, 180)
    );

    /// <summary>
    /// Default text color theme for buttons (Normal, Hover, Disabled).
    /// </summary>
    public static readonly ButtonStateColors TextTheme = new(
        new Color(200, 200, 200),
        new Color(255, 255, 255),
        new Color(160, 160, 160, 200)
    );

    /// <summary>
    /// Default spinner color.
    /// </summary>
    public static readonly Color SpinnerForegroundColor = new(255, 255, 255);

    /// <summary>
    /// Default banner background color (black, alpha 100).
    /// </summary>
    public static readonly Color BannerBackgroundColor = new(0, 0, 0, 100);

    /// <summary>
    /// Default text color (white, opaque).
    /// </summary>
    public static readonly Color PrimaryTextColor = new(255, 255, 255);
}
