using Ascendance.Tiled.Abstractions;
using Ascendance.Tiled.Collections;
using System.Xml.Linq;

namespace Ascendance.Tiled.Tilesets;

/// <summary>
/// Represents a terrain entry declared in &lt;terraintypes&gt;.
/// </summary>
public class TmxTerrain : ITmxElement
{
    /// <summary>
    /// Terrain name.
    /// </summary>
    public System.String Name { get; private set; }

    /// <summary>
    /// Representative tile id for this terrain (local tile id).
    /// </summary>
    public System.Int32 Tile { get; private set; }

    /// <summary>
    /// Custom properties for this terrain.
    /// </summary>
    public PropertyDict Properties { get; private set; }

    public TmxTerrain(XElement xTerrain)
    {
        System.ArgumentNullException.ThrowIfNull(xTerrain);

        Name = (System.String)xTerrain.Attribute("name") ?? System.String.Empty;
        Tile = (System.Int32?)xTerrain.Attribute("tile") ?? 0;
        Properties = new PropertyDict(xTerrain.Element("properties"));
    }
}