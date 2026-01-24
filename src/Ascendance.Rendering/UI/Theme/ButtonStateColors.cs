// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.UI.Theme;

/// <summary>
/// Defines a set of colors for a button's visual states: Normal, Hover, and Disabled.
/// </summary>
public class ButtonStateColors
{
    /// <summary>
    /// Gets or sets the color used for the normal (default) state.
    /// </summary>
    public Color Normal { get; set; }

    /// <summary>
    /// Gets or sets the color used when the mouse is hovering over the button.
    /// </summary>
    public Color Hover { get; set; }

    /// <summary>
    /// Gets or sets the color used when the button is disabled.
    /// </summary>
    public Color Disabled { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonStateColors"/> class with custom colors for each state.
    /// </summary>
    /// <param name="normal">Color for the normal state.</param>
    /// <param name="hover">Color for the hover state.</param>
    /// <param name="disabled">Color for the disabled state.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public ButtonStateColors(Color normal, Color hover, Color disabled)
    {
        Hover = hover;
        Normal = normal;
        Disabled = disabled;
    }
}