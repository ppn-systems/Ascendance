using System.Globalization;
using System.Xml.Linq;

namespace Ascendance.Tiled.Core;

/// <summary>
/// Simple RGB color parsed from a hex string like "#RRGGBB".
/// </summary>
public class TmxColor
{
    public System.Int32 R { get; private set; }
    public System.Int32 G { get; private set; }
    public System.Int32 B { get; private set; }

    /// <summary>
    /// Parse a hex color attribute. If xColor is null, color remains (0,0,0).
    /// </summary>
    /// <param name="xColor">Attribute containing a color string (e.g. "#ff00aa").</param>
    public TmxColor(XAttribute xColor)
    {
        if (xColor == null)
        {
            return;
        }

        var colorStr = (xColor.Value ?? System.String.Empty).TrimStart('#').Trim();

        // Expect at least 6 hex digits RRGGBB
        if (colorStr.Length < 6)
        {
            return;
        }

        R = System.Int32.Parse(colorStr[..2], NumberStyles.HexNumber);
        G = System.Int32.Parse(colorStr.Substring(2, 2), NumberStyles.HexNumber);
        B = System.Int32.Parse(colorStr.Substring(4, 2), NumberStyles.HexNumber);
    }
}