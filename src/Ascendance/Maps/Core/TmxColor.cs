// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Maps.Core;

/// <summary>
/// Simple RGB color parsed from a hex string like "#RRGGBB".
/// </summary>
public class TmxColor
{
    public System.Int32 R { get; }
    public System.Int32 G { get; }
    public System.Int32 B { get; }

    /// <summary>
    /// Parse a hex color attribute. If xColor is null, color remains (0,0,0).
    /// </summary>
    /// <param name="xColor">Attribute containing a color string (e.g. "#ff00aa").</param>
    public TmxColor(System.Xml.Linq.XAttribute xColor)
    {
        if (xColor == null)
        {
            return;
        }

        System.String colorStr = (xColor.Value ?? System.String.Empty).TrimStart('#').Trim();

        // Expect at least 6 hex digits RRGGBB
        if (colorStr.Length < 6)
        {
            return;
        }

        R = System.Int32.Parse(colorStr[..2], System.Globalization.NumberStyles.HexNumber);
        G = System.Int32.Parse(colorStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        B = System.Int32.Parse(colorStr.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
    }
}