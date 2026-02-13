// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.Maps.Collections;
using Ascendance.Maps.Core;
using Ascendance.Maps.Enums;
using Ascendance.Maps.Layers;
using Ascendance.Maps.Tilesets;
using System.Linq;

namespace Ascendance.Maps;

/// <summary>
/// Represents a TMX map document parsed from Tiled.
/// </summary>
public class TmxMap : TmxDocument
{
    #region Fields

    // Static maps to avoid reallocating dictionaries on each Load call.
    private static readonly System.Collections.Generic.Dictionary<System.String, OrientationType> s_orientDict = new()
    {
        ["unknown"] = OrientationType.Unknown,
        ["isometric"] = OrientationType.Isometric,
        ["staggered"] = OrientationType.Staggered,
        ["hexagonal"] = OrientationType.Hexagonal,
        ["orthogonal"] = OrientationType.Orthogonal,
    };

    private static readonly System.Collections.Generic.Dictionary<System.String, StaggerAxisType> s_staggerAxisDict = new()
    {
        ["x"] = StaggerAxisType.X,
        ["y"] = StaggerAxisType.Y,
    };

    private static readonly System.Collections.Generic.Dictionary<System.String, StaggerIndexType> s_staggerIndexDict = new()
    {
        ["odd"] = StaggerIndexType.Odd,
        ["even"] = StaggerIndexType.Even,
    };

    private static readonly System.Collections.Generic.Dictionary<System.String, RenderOrderType> s_renderDict = new()
    {
        ["left-up"] = RenderOrderType.LeftUp,
        ["right-up"] = RenderOrderType.RightUp,
        ["left-down"] = RenderOrderType.LeftDown,
        ["right-down"] = RenderOrderType.RightDown,
    };

    #endregion Fields

    #region Properties

    /// <summary>
    /// TMX version string from the <c>version</c> attribute.
    /// </summary>
    public System.String Version { get; private set; }

    /// <summary>
    /// Tiled editor version string from the <c>tiledversion</c> attribute.
    /// </summary>
    public System.String TiledVersion { get; private set; }

    /// <summary>
    /// Map width in tiles.
    /// </summary>
    public System.Int32 Width { get; private set; }

    /// <summary>
    /// Map height in tiles.
    /// </summary>
    public System.Int32 Height { get; private set; }

    /// <summary>
    /// Tile width in pixels.
    /// </summary>
    public System.Int32 TileWidth { get; private set; }

    /// <summary>
    /// Tile height in pixels.
    /// </summary>
    public System.Int32 TileHeight { get; private set; }

    /// <summary>
    /// Optional hex side length (pixels) for hexagonal maps.
    /// </summary>
    public System.Int32? HexSideLength { get; private set; }

    /// <summary>
    /// Map orientation (orthogonal, isometric, etc.).
    /// </summary>
    public OrientationType Orientation { get; private set; }

    /// <summary>
    /// Stagger axis for hex/staggered maps.
    /// </summary>
    public StaggerAxisType StaggerAxis { get; private set; }

    /// <summary>
    /// Stagger index for hex/staggered maps.
    /// </summary>
    public StaggerIndexType StaggerIndex { get; private set; }

    /// <summary>
    /// Tile draw/render order.
    /// </summary>
    public RenderOrderType RenderOrder { get; private set; }

    /// <summary>
    /// Optional background color specified on the map.
    /// </summary>
    public TmxColor BackgroundColor { get; private set; }

    /// <summary>
    /// Next object id (if present in the TMX).
    /// </summary>
    public System.Int32? NextObjectID { get; private set; }

    /// <summary>
    /// Tilesets referenced by this map (in document order).
    /// </summary>
    public TmxList<TmxTileset> Tilesets { get; private set; }

    /// <summary>
    /// Tile layers directly on the map (not nested in groups).
    /// </summary>
    public TmxList<TmxLayer> TileLayers { get; private set; }

    /// <summary>
    /// Object groups directly on the map.
    /// </summary>
    public TmxList<TmxObjectGroup> ObjectGroups { get; private set; }

    /// <summary>
    /// Image layers directly on the map.
    /// </summary>
    public TmxList<TmxImageLayer> ImageLayers { get; private set; }

    /// <summary>
    /// Groups directly on the map.
    /// </summary>
    public TmxList<TmxGroup> Groups { get; private set; }

    /// <summary>
    /// All layers on the map in document order.
    /// </summary>
    public TmxList<ITmxLayer> Layers { get; private set; }

    /// <summary>
    /// Custom properties attached to the map.
    /// </summary>
    public PropertyDict Properties { get; private set; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Load a TMX from file path.
    /// </summary>
    /// <param name="filename">Path to the TMX file.</param>
    /// <param name="customLoader">Optional custom loader for external resources.</param>
    public TmxMap(System.String filename, ICustomLoader customLoader = null) : base(customLoader)
    {
        if (System.String.IsNullOrWhiteSpace(filename))
        {
            throw new System.ArgumentException("filename must be provided", nameof(filename));
        }

        LOAD(ReadXml(filename));
    }

    /// <summary>
    /// Load a TMX from a stream.
    /// </summary>
    /// <param name="inputStream">Stream containing TMX XML.</param>
    /// <param name="customLoader">Optional custom loader.</param>
    public TmxMap(System.IO.Stream inputStream, ICustomLoader customLoader = null) : base(customLoader)
    {
        System.ArgumentNullException.ThrowIfNull(inputStream);

        using System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(inputStream);
        LOAD(System.Xml.Linq.XDocument.Load(xmlReader));
    }

    #endregion Constructors

    #region Private Methods

    /// <summary>
    /// Load a TMX from an already-parsed XDocument.
    /// </summary>
    /// <param name="xDoc">Parsed TMX document.</param>
    /// <param name="customLoader">Optional custom loader.</param>
    public TmxMap(System.Xml.Linq.XDocument xDoc, ICustomLoader customLoader = null) : base(customLoader) => LOAD(xDoc);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
    private void LOAD(System.Xml.Linq.XDocument xDoc)
    {
        System.ArgumentNullException.ThrowIfNull(xDoc);

        System.Xml.Linq.XElement xMap = xDoc.Element("map")
            ?? throw new System.InvalidOperationException("The TMX document does not contain a <map> root element.");

        Version = (System.String)xMap.Attribute("version") ?? System.String.Empty;
        TiledVersion = (System.String)xMap.Attribute("tiledversion") ?? System.String.Empty;

        Width = (System.Int32?)xMap.Attribute("width") ?? throw new System.InvalidOperationException("<map> is missing required attribute 'width'.");
        Height = (System.Int32?)xMap.Attribute("height") ?? throw new System.InvalidOperationException("<map> is missing required attribute 'height'.");
        TileWidth = (System.Int32?)xMap.Attribute("tilewidth") ?? throw new System.InvalidOperationException("<map> is missing required attribute 'tilewidth'.");
        TileHeight = (System.Int32?)xMap.Attribute("tileheight") ?? throw new System.InvalidOperationException("<map> is missing required attribute 'tileheight'.");

        if (Width < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(Width));
        }

        if (Height < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(Height));
        }

        if (TileWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(TileWidth));
        }

        if (TileHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(this.TileHeight));
        }

        HexSideLength = (System.Int32?)xMap.Attribute("hexsidelength");

        System.String orientValue = (System.String)xMap.Attribute("orientation");
        Orientation = orientValue != null && s_orientDict.TryGetValue(orientValue, out var orientation) ? orientation : OrientationType.Unknown;

        System.String staggerAxisValue = (System.String)xMap.Attribute("staggeraxis");
        if (staggerAxisValue != null && s_staggerAxisDict.TryGetValue(staggerAxisValue, out var sa))
        {
            StaggerAxis = sa;
        }

        System.String staggerIndexValue = (System.String)xMap.Attribute("staggerindex");
        if (staggerIndexValue != null && s_staggerIndexDict.TryGetValue(staggerIndexValue, out var si))
        {
            StaggerIndex = si;
        }

        System.String renderValue = (System.String)xMap.Attribute("renderorder");
        if (renderValue != null && s_renderDict.TryGetValue(renderValue, out var ro))
        {
            RenderOrder = ro;
        }
        else
        {
            RenderOrder = RenderOrderType.RightDown; // sensible default if absent
        }

        NextObjectID = (System.Int32?)xMap.Attribute("nextobjectid");
        BackgroundColor = new TmxColor(xMap.Attribute("backgroundcolor"));

        Properties = new PropertyDict(xMap.Element("properties"));

        // Tilesets
        Tilesets = [];
        foreach (var e in xMap.Elements("tileset"))
        {
            Tilesets.Add(new TmxTileset(e, TmxDirectory, CustomLoader));
        }

        // Layers (preserve document order)
        Layers = [];
        TileLayers = [];
        ObjectGroups = [];
        ImageLayers = [];
        Groups = [];

        var relevantElements = xMap.Elements()
                                   .Where(x =>
                                   {
                                       var n = x.Name.LocalName;
                                       return n is "layer" or "objectgroup" or "imagelayer" or "group";
                                   });

        foreach (var e in relevantElements)
        {
            ITmxLayer layer = e.Name.LocalName switch
            {
                "layer" => CREATE_AND_ADD_TILE_LAYER(e),
                "objectgroup" => CREATE_AND_ADD_OBJECT_GROUP(e),
                "imagelayer" => CREATE_AND_ADD_IMAGE_LAYER(e),
                "group" => CREATE_AND_ADD_GROUP(e),
                _ => throw new System.InvalidOperationException($"Unexpected layer element: {e.Name.LocalName}")
            };

            Layers.Add(layer);
        }
    }

    private TmxGroup CREATE_AND_ADD_GROUP(System.Xml.Linq.XElement e)
    {
        TmxGroup group = new(e, Width, Height, TmxDirectory);
        Groups.Add(group);
        return group;
    }

    private TmxLayer CREATE_AND_ADD_TILE_LAYER(System.Xml.Linq.XElement e)
    {
        TmxLayer tileLayer = new(e, Width, Height);
        TileLayers.Add(tileLayer);
        return tileLayer;
    }

    private TmxImageLayer CREATE_AND_ADD_IMAGE_LAYER(System.Xml.Linq.XElement e)
    {
        TmxImageLayer imageLayer = new(e, TmxDirectory);
        ImageLayers.Add(imageLayer);
        return imageLayer;
    }

    private TmxObjectGroup CREATE_AND_ADD_OBJECT_GROUP(System.Xml.Linq.XElement e)
    {
        TmxObjectGroup objectGroup = new(e);
        ObjectGroups.Add(objectGroup);
        return objectGroup;
    }

    #endregion Private Methods
}