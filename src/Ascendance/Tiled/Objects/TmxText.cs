// Copyright (c) 2025 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Tiled.Core;

namespace Ascendance.Tiled.Objects;

/// <summary>
/// Represents text embedded in a Tiled &lt;text&gt; element.
/// </summary>
public class TmxText
{
    /// <summary>
    /// Font family name. Defaults to "sans-serif" when not specified.
    /// </summary>
    public System.String FontFamily { get; }

    /// <summary>
    /// Font size in pixels. Defaults to 16 when not specified or invalid.
    /// </summary>
    public System.Int32 PixelSize { get; }

    /// <summary>
    /// Whether the text is wrapped. Defaults to false.
    /// </summary>
    public System.Boolean Wrap { get; }

    /// <summary>
    /// Text color. Constructed from the "color" attribute if present.
    /// </summary>
    public TmxColor Color { get; }

    /// <summary>
    /// Bold flag.
    /// </summary>
    public System.Boolean Bold { get; }

    /// <summary>
    /// Italic flag.
    /// </summary>
    public System.Boolean Italic { get; }

    /// <summary>
    /// Underline flag.
    /// </summary>
    public System.Boolean Underline { get; }

    /// <summary>
    /// Strikeout flag.
    /// </summary>
    public System.Boolean Strikeout { get; }

    /// <summary>
    /// Kerning enabled. Defaults to true.
    /// </summary>
    public System.Boolean Kerning { get; }

    /// <summary>
    /// Horizontal and vertical alignment.
    /// </summary>
    public TmxAlignment Alignment { get; }

    /// <summary>
    /// The text content (inner text of the &lt;text&gt; element). May be empty.
    /// </summary>
    public System.String Value { get; }

    /// <summary>
    /// Parses a &lt;text&gt; element into a <see cref="TmxText"/> instance.
    /// </summary>
    /// <param name="xText">The &lt;text&gt; element to parse. Must not be null.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xText"/> is null.</exception>
    public TmxText(System.Xml.Linq.XElement xText)
    {
        System.ArgumentNullException.ThrowIfNull(xText);

        FontFamily = (System.String)xText.Attribute("fontfamily") ?? "sans-serif";

        // PixelSize: keep a sensible default if missing or invalid (<= 0)
        var pixelSize = (System.Int32?)xText.Attribute("pixelsize") ?? 16;
        PixelSize = pixelSize > 0 ? pixelSize : 16;

        Wrap = (System.Boolean?)xText.Attribute("wrap") ?? false;

        Color = new TmxColor(xText.Attribute("color"));

        Bold = (System.Boolean?)xText.Attribute("bold") ?? false;
        Italic = (System.Boolean?)xText.Attribute("italic") ?? false;
        Underline = (System.Boolean?)xText.Attribute("underline") ?? false;
        Strikeout = (System.Boolean?)xText.Attribute("strikeout") ?? false;

        Kerning = (System.Boolean?)xText.Attribute("kerning") ?? true;

        Alignment = new TmxAlignment(xText.Attribute("halign"), xText.Attribute("valign"));

        Value = xText.Value ?? System.String.Empty;
    }
}