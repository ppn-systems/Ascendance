// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Maps.Tilesets;

/// <summary>
/// Represents an optional per-tileset pixel offset used when rendering tiles.
/// </summary>
public class TmxTileOffset
{
    #region Properties

    /// <summary>
    /// X offset in pixels.
    /// </summary>
    public System.Int32 X { get; }

    /// <summary>
    /// Y offset in pixels.
    /// </summary>
    public System.Int32 Y { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Parses a &lt;tileoffset&gt; element. When the element is null, offset defaults to (0,0).
    /// </summary>
    /// <param name="xTileOffset">The &lt;tileoffset&gt; element or null.</param>
    public TmxTileOffset(System.Xml.Linq.XElement xTileOffset)
    {
        if (xTileOffset == null)
        {
            X = 0;
            Y = 0;
        }
        else
        {
            X = (System.Int32?)xTileOffset.Attribute("x") ?? 0;
            Y = (System.Int32?)xTileOffset.Attribute("y") ?? 0;
        }
    }

    #endregion Constructor
}
