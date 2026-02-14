// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Enums;

namespace Ascendance.Maps.Objects;

/// <summary>
/// Represents horizontal and vertical alignment parsed from TMX attributes.
/// </summary>
/// <remarks>
/// Parses alignment from the given XML attributes.
/// Parsing is case-insensitive and falls back to sensible defaults when attributes are absent or cannot be parsed.
/// </remarks>
/// <param name="halign">The horizontal alignment attribute (may be null).</param>
/// <param name="valign">The vertical alignment attribute (may be null).</param>
public class TmxAlignment(System.Xml.Linq.XAttribute halign, System.Xml.Linq.XAttribute valign)
{
    /// <summary>
    /// Vertical alignment. Defaults to <see cref="TmxVerticalAlignment.Top"/> when attribute is missing or invalid.
    /// </summary>
    public TmxVerticalAlignment Vertical { get; } = valign != null
        && System.Enum.TryParse(valign.Value, ignoreCase: true, out TmxVerticalAlignment v) ? v : TmxVerticalAlignment.Top;

    /// <summary>
    /// Horizontal alignment. Defaults to <see cref="TmxHorizontalAlignment.Left"/> when attribute is missing or invalid.
    /// </summary>
    public TmxHorizontalAlignment Horizontal { get; } = halign != null
        && System.Enum.TryParse(halign.Value, ignoreCase: true, out TmxHorizontalAlignment h) ? h : TmxHorizontalAlignment.Left;
}