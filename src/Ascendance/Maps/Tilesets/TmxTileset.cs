// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.Maps.Collections;
using Ascendance.Maps.Core;

namespace Ascendance.Maps.Tilesets;

/// <summary>
/// Represents a tileset defined in TMX or TSX format.
/// This class can be constructed from a &lt;tileset&gt; element (embedded in a TMX)
/// or from a TSX document (XContainer).
/// </summary>
public class TmxTileset : TmxDocument, ITmxElement
{
    #region Properties

    /// <summary>
    /// The first global tile id this tileset maps to (present on TMX references).
    /// May be 0 when absent (e.g. when parsing a standalone TSX file).
    /// </summary>
    public System.Int32 FirstGid { get; }

    /// <summary>
    /// Tileset name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Tile width in pixels.
    /// </summary>
    public System.Int32 TileWidth { get; }

    /// <summary>
    /// Tile height in pixels.
    /// </summary>
    public System.Int32 TileHeight { get; }

    /// <summary>
    /// Spacing between tiles in the source image (pixels).
    /// </summary>
    public System.Int32 Spacing { get; }

    /// <summary>
    /// Margin around the tiles in the source image (pixels).
    /// </summary>
    public System.Int32 Margin { get; }

    /// <summary>
    /// Number of columns in the source image, if present.
    /// </summary>
    public System.Int32? Columns { get; }

    /// <summary>
    /// Number of tiles in the tileset, if present.
    /// </summary>
    public System.Int32? TileCount { get; }

    /// <summary>
    /// Map of local tile id -> tileset tile information (tile-specific data).
    /// </summary>
    public System.Collections.Generic.Dictionary<System.Int32, TmxTilesetTile> Tiles { get; }

    /// <summary>
    /// Optional tile offset for rendering.
    /// </summary>
    public TmxTileOffset TileOffset { get; }

    /// <summary>
    /// Custom properties attached to the tileset.
    /// </summary>
    public PropertyDict Properties { get; }

    /// <summary>
    /// Optional image referenced by the tileset (if not a collection of images).
    /// </summary>
    public TmxImage Image { get; }

    /// <summary>
    /// Terrains declared in the tileset (terraintypes).
    /// </summary>
    public TmxList<TmxTerrain> Terrains { get; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Constructor for parsing a TSX document (XContainer) or an XElement that wraps a tileset element.
    /// </summary>
    /// <param name="xDoc">TSX document or container that contains a &lt;tileset&gt; element.</param>
    /// <param name="tmxDir">Base directory for resolving relative image/source paths.</param>
    /// <param name="customLoader">Optional custom loader for external resources.</param>
    public TmxTileset(System.Xml.Linq.XContainer xDoc, System.String tmxDir, ICustomLoader customLoader = null)
        : this(xDoc?.Element("tileset") ?? throw new System.ArgumentException("TSX document does not contain <tileset> element.", nameof(xDoc)), tmxDir, customLoader)
    {
    }

    /// <summary>
    /// Parses a &lt;tileset&gt; element. Handles both embedded tileset elements in TMX
    /// and tileset references that point to external TSX files via the "source" attribute.
    /// </summary>
    /// <param name="xTileset">The &lt;tileset&gt; element to parse.</param>
    /// <param name="tmxDir">Base directory for resolving relative paths (images/TSX).</param>
    /// <param name="customLoader">Optional custom loader.</param>
    public TmxTileset(System.Xml.Linq.XElement xTileset, System.String tmxDir = "", ICustomLoader customLoader = null)
        : base(customLoader)
    {
        System.ArgumentNullException.ThrowIfNull(xTileset);

        // Read optional firstgid attribute (present when tileset is referenced from a TMX).
        System.Xml.Linq.XAttribute xFirstGid = xTileset.Attribute("firstgid");
        System.String source = (System.String)xTileset.Attribute("source");

        // If source is present, the tileset is external (TSX). Load TSX and copy its data.
        if (!System.String.IsNullOrEmpty(source))
        {
            System.String sourcePath = System.IO.Path.IsPathRooted(source) ? source : System.IO.Path.Combine(tmxDir ?? System.String.Empty, source);
            if (!System.IO.File.Exists(sourcePath))
            {
                throw new System.IO.FileNotFoundException("Referenced TSX file not found.", sourcePath);
            }

            // firstgid may be present on the TMX side - keep it if so.
            FirstGid = xFirstGid != null ? (System.Int32)xFirstGid : 0;

            // Load TSX content and create a tileset from it (TSX won't have 'source' attribute).
            System.Xml.Linq.XDocument tsxDoc = ReadXml(sourcePath);
            // Ensure tileset inside TSX is used; pass the directory of the TSX for resolving image paths.
            TmxTileset tsxTileset = new(tsxDoc, System.IO.Path.GetDirectoryName(sourcePath) ?? tmxDir, customLoader);

            // Copy values from the loaded TSX tileset.
            Name = tsxTileset.Name;
            TileWidth = tsxTileset.TileWidth;
            TileHeight = tsxTileset.TileHeight;
            Spacing = tsxTileset.Spacing;
            Margin = tsxTileset.Margin;
            Columns = tsxTileset.Columns;
            TileCount = tsxTileset.TileCount;
            TileOffset = tsxTileset.TileOffset;
            Image = tsxTileset.Image;
            Terrains = tsxTileset.Terrains;
            Tiles = tsxTileset.Tiles;
            Properties = tsxTileset.Properties;
            return;
        }

        // Embedded tileset (or standalone TSX parsed via XElement)
        FirstGid = xFirstGid != null ? (System.Int32)xFirstGid : 0;

        Name = (System.String)xTileset.Attribute("name") ?? System.String.Empty;
        TileWidth = (System.Int32?)xTileset.Attribute("tilewidth") ?? throw new System.InvalidOperationException("<tileset> missing required attribute 'tilewidth'.");
        TileHeight = (System.Int32?)xTileset.Attribute("tileheight") ?? throw new System.InvalidOperationException("<tileset> missing required attribute 'tileheight'.");
        Spacing = (System.Int32?)xTileset.Attribute("spacing") ?? 0;
        Margin = (System.Int32?)xTileset.Attribute("margin") ?? 0;
        Columns = (System.Int32?)xTileset.Attribute("columns");
        TileCount = (System.Int32?)xTileset.Attribute("tilecount");

        TileOffset = new TmxTileOffset(xTileset.Element("tileoffset"));
        Image = new TmxImage(xTileset.Element("image"), tmxDir);

        // Terrains
        Terrains = [];
        System.Xml.Linq.XElement xTerrainTypes = xTileset.Element("terraintypes");
        if (xTerrainTypes != null)
        {
            foreach (var e in xTerrainTypes.Elements("terrain"))
            {
                Terrains.Add(new TmxTerrain(e));
            }
        }

        // Tiles (tile-specific overrides)
        Tiles = [];
        foreach (System.Xml.Linq.XElement xTile in xTileset.Elements("tile"))
        {
            TmxTilesetTile tile = new(xTile, Terrains, tmxDir);
            Tiles[tile.Id] = tile;
        }

        Properties = new PropertyDict(xTileset.Element("properties"));
    }

    #endregion Constructors
}
