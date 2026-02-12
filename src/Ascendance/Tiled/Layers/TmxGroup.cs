// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Tiled.Abstractions;
using Ascendance.Tiled.Collections;
using Ascendance.Tiled.Core;
using System.Linq;

namespace Ascendance.Tiled.Layers;

/// <summary>
/// Represents a Tiled "group" layer which can contain other layers (tile layers,
/// object groups, image layers or nested groups).
/// </summary>
public class TmxGroup : ITmxLayer
{
    #region Properties

    /// <summary>
    /// The group name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// The group's opacity (0.0 - 1.0).
    /// </summary>
    public System.Double Opacity { get; }

    /// <summary>
    /// Whether the group is visible.
    /// </summary>
    public System.Boolean Visible { get; }

    /// <summary>
    /// Horizontal offset in pixels.
    /// </summary>
    public System.Double? OffsetX { get; }

    /// <summary>
    /// Vertical offset in pixels.
    /// </summary>
    public System.Double? OffsetY { get; }

    /// <summary>
    /// All child layers in document order.
    /// </summary>
    public TmxList<ITmxLayer> Layers { get; }

    /// <summary>
    /// Tile layers directly contained by this group.
    /// </summary>
    public TmxList<TmxLayer> TileLayers { get; }

    /// <summary>
    /// Object groups directly contained by this group.
    /// </summary>
    public TmxList<TmxObjectGroup> ObjectGroups { get; }

    /// <summary>
    /// Image layers directly contained by this group.
    /// </summary>
    public TmxList<TmxImageLayer> ImageLayers { get; }

    /// <summary>
    /// Nested groups directly contained by this group.
    /// </summary>
    public TmxList<TmxGroup> Groups { get; }

    /// <summary>
    /// Custom properties for this group.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of <see cref="TmxGroup"/> from the provided XML element.
    /// </summary>
    /// <param name="xGroup">The XML element representing the <c>group</c>.</param>
    /// <param name="width">Map width in tiles (passed to tile layers).</param>
    /// <param name="height">Map height in tiles (passed to tile layers).</param>
    /// <param name="tmxDirectory">Directory of the TMX file, used for resolving image paths.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xGroup"/> is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is negative.</exception>
    public TmxGroup(System.Xml.Linq.XElement xGroup, System.Int32 width, System.Int32 height, System.String tmxDirectory)
    {
        System.ArgumentNullException.ThrowIfNull(xGroup);
        System.ArgumentOutOfRangeException.ThrowIfNegative(width);
        System.ArgumentOutOfRangeException.ThrowIfNegative(height);

        this.Opacity = (System.Double?)xGroup.Attribute("opacity") ?? 1.0;
        this.OffsetX = (System.Double?)xGroup.Attribute("offsetx") ?? 0.0;
        this.OffsetY = (System.Double?)xGroup.Attribute("offsety") ?? 0.0;
        this.Visible = (System.Boolean?)xGroup.Attribute("visible") ?? true;
        this.Name = (System.String)xGroup.Attribute("name") ?? System.String.Empty;

        this.Properties = new PropertyDict(xGroup.Element("properties"));

        // Pre-allocate lists. If TmxList supports capacity overload, consider using it.
        this.Layers = [];
        this.Groups = [];
        this.TileLayers = [];
        this.ImageLayers = [];
        this.ObjectGroups = [];

        // Filter only relevant child elements and preserve document order.
        var relevantElements = xGroup.Elements()
                                     .Where(e =>
                                     {
                                         var n = e.Name.LocalName;
                                         return n is "layer" or "objectgroup" or "imagelayer" or "group";
                                     });

        foreach (System.Xml.Linq.XElement e in relevantElements)
        {
            // Use LocalName to avoid XName equality pitfalls.
            ITmxLayer layer = e.Name.LocalName switch
            {
                "layer" => CREATE_AND_ADD_TILE_LAYER(e, width, height),
                "objectgroup" => CREATE_AND_ADD_OBJECT_GROUP(e),
                "imagelayer" => CREATE_AND_ADD_IMAGE_LAYER(e, tmxDirectory),
                "group" => CREATE_AND_ADD_GROUP(e, width, height, tmxDirectory),
                _ => throw new System.InvalidOperationException($"Unexpected layer element: {e.Name.LocalName}")
            };

            this.Layers.Add(layer);
        }
    }

    #endregion Constructor

    #region Private Methods

    private TmxLayer CREATE_AND_ADD_TILE_LAYER(System.Xml.Linq.XElement e, System.Int32 width, System.Int32 height)
    {
        TmxLayer tileLayer = new(e, width, height);
        TileLayers.Add(tileLayer);
        return tileLayer;
    }

    private TmxObjectGroup CREATE_AND_ADD_OBJECT_GROUP(System.Xml.Linq.XElement e)
    {
        TmxObjectGroup objectGroup = new(e);
        ObjectGroups.Add(objectGroup);
        return objectGroup;
    }

    private TmxImageLayer CREATE_AND_ADD_IMAGE_LAYER(System.Xml.Linq.XElement e, System.String tmxDirectory)
    {
        TmxImageLayer imageLayer = new(e, tmxDirectory);
        ImageLayers.Add(imageLayer);
        return imageLayer;
    }

    private TmxGroup CREATE_AND_ADD_GROUP(System.Xml.Linq.XElement e, System.Int32 width, System.Int32 height, System.String tmxDirectory)
    {
        TmxGroup group = new(e, width, height, tmxDirectory);
        Groups.Add(group);
        return group;
    }

    #endregion Private Methods
}