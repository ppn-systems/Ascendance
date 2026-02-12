// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Abstractions;
using Ascendance.Maps.Collections;

namespace Ascendance.Maps.Tilesets;

/// <summary>
/// Represents a terrain entry declared in &lt;terraintypes&gt;.
/// </summary>
public class TmxTerrain : ITmxElement
{
    #region Properties

    /// <summary>
    /// Terrain name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Representative tile id for this terrain (local tile id).
    /// </summary>
    public System.Int32 Tile { get; }

    /// <summary>
    /// Custom properties for this terrain.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Constructor

    public TmxTerrain(System.Xml.Linq.XElement xTerrain)
    {
        System.ArgumentNullException.ThrowIfNull(xTerrain);

        Name = (System.String)xTerrain.Attribute("name") ?? System.String.Empty;
        Tile = (System.Int32?)xTerrain.Attribute("tile") ?? 0;
        Properties = new PropertyDict(xTerrain.Element("properties"));
    }

    #endregion Constructor
}