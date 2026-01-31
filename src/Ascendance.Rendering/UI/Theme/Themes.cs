// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.UI.Theme;

/// <summary>
/// Defines a collection of predefined color themes used by UI components.
/// <para>
/// These themes provide consistent visual styling for buttons, text,
/// banners, and loading indicators across the rendering system.
/// </para>
/// </summary>
public static class Themes
{
    /// <summary>
    /// Gets the default panel color theme for buttons.
    /// </summary>
    /// <remarks>
    /// The colors are applied according to the button state:
    /// <list type="bullet">
    ///   <item><description><b>Normal</b>: Default idle state.</description></item>
    ///   <item><description><b>Hover</b>: Mouse-over state.</description></item>
    ///   <item><description><b>Disabled</b>: Non-interactive state.</description></item>
    /// </list>
    /// </remarks>
    public static readonly ButtonStateColors PanelTheme = new(
        new Color(30, 30, 30),
        new Color(60, 60, 60),
        new Color(40, 40, 40, 180)
    );

    /// <summary>
    /// Gets the default text color theme for buttons.
    /// </summary>
    /// <remarks>
    /// The color set corresponds to the button interaction states:
    /// <list type="bullet">
    ///   <item><description><b>Normal</b>: Standard readable text.</description></item>
    ///   <item><description><b>Hover</b>: Highlighted text on hover.</description></item>
    ///   <item><description><b>Disabled</b>: Muted text for disabled buttons.</description></item>
    /// </list>
    /// </remarks>
    public static readonly ButtonStateColors TextTheme = new(
        new Color(200, 200, 200),
        new Color(255, 255, 255),
        new Color(160, 160, 160, 200)
    );

    /// <summary>
    /// Gets the primary text color used across the UI.
    /// </summary>
    /// <remarks>
    /// This color represents fully opaque white and is typically used
    /// for high-contrast foreground text.
    /// </remarks>
    public static readonly Color PrimaryTextColor = new(255, 255, 255);

    /// <summary>
    /// Gets the default background color for banner-style UI elements.
    /// </summary>
    /// <remarks>
    /// Uses a semi-transparent black color to ensure readability
    /// while preserving background visibility.
    /// </remarks>
    public static readonly Color BannerBackgroundColor = new(0, 0, 0, 100);

    /// <summary>
    /// Gets the default foreground color for loading spinners.
    /// </summary>
    /// <remarks>
    /// Designed to be clearly visible on dark backgrounds.
    /// </remarks>
    public static readonly Color SpinnerForegroundColor = new(255, 255, 255);
}
