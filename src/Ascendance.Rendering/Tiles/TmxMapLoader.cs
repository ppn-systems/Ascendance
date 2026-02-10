// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Managers;
using Nalix.Logging.Extensions;
using System.Xml.Linq;

namespace Ascendance.Rendering.Tiles;

/// <summary>
/// Provides static methods for loading tile maps from Tiled Map Editor (.tmx) XML files.
/// </summary>
/// <remarks>
/// <para>
/// This utility class parses TMX (Tile Map XML) files created by the Tiled Map Editor
/// and constructs <see cref="TileMap"/> objects with all associated layers, tilesets, and tile data.
/// </para>
/// <para>
/// Supported features:
/// </para>
/// <list type="bullet">
/// <item>Multiple tilesets per map</item>
/// <item>Multiple tile layers with visibility and opacity settings</item>
/// <item>CSV and XML tile data encoding formats</item>
/// <item>Custom tile properties (collision, metadata)</item>
/// <item>Tileset spacing and margin support</item>
/// <item>Layer-level and tile-level custom properties</item>
/// </list>
/// <para>
/// All methods are stateless and thread-safe for read-only asset operations.
/// </para>
/// </remarks>
public static class TmxMapLoader
{
    /// <summary>
    /// Loads a complete tile map from a TMX file, including all tilesets and layers.
    /// </summary>
    /// <param name="tmxPath">The absolute or relative file path to the .tmx file to load.</param>
    /// <param name="assetManager">
    /// The <see cref="AssetManager"/> instance used to load tileset textures.
    /// Must not be <c>null</c>.
    /// </param>
    /// <returns>
    /// A fully initialized <see cref="TileMap"/> instance with all layers and tilesets loaded
    /// and vertex arrays built, or <c>null</c> if loading fails due to invalid file format
    /// or I/O errors.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item>Parses the TMX XML structure to extract map dimensions and tile size.</item>
    /// <item>Loads all referenced tilesets with their textures and tile properties.</item>
    /// <item>Loads all tile layers with their tile data (supports CSV and XML encoding).</item>
    /// <item>Builds vertex arrays for all layers to prepare for rendering via <see cref="TileMap.BuildAllLayers"/>.</item>
    /// </list>
    /// <para>
    /// Texture paths in the TMX file are resolved relative to the TMX file's directory.
    /// All parsing errors and warnings are logged using the Nalix logging framework with source "TmxMapLoader".
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="assetManager"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// AssetManager assets = new AssetManager();
    /// TileMap map = TmxMapLoader.Load("Assets/Maps/Level1.tmx", assets);
    /// if (map != null)
    /// {
    ///     // Map loaded successfully
    ///     map.Draw(renderTarget);
    /// }
    /// </code>
    /// </example>
    public static TileMap Load(System.String tmxPath, AssetManager assetManager)
    {
        try
        {
            XDocument doc = XDocument.Load(tmxPath);
            XElement mapElement = doc.Element("map");

            if (mapElement == null)
            {
                $"Invalid TMX file: {tmxPath}".Error(source: "TmxMapLoader");
                return null;
            }

            // Parse map properties
            System.Int16 width = System.Int16.Parse(mapElement.Attribute("width")?.Value ?? "0");
            System.Int16 height = System.Int16.Parse(mapElement.Attribute("height")?.Value ?? "0");
            System.Int16 tileWidth = System.Int16.Parse(mapElement.Attribute("tilewidth")?.Value ?? "0");
            System.Int16 tileHeight = System.Int16.Parse(mapElement.Attribute("tileheight")?.Value ?? "0");

            TileMap tileMap = new(width, height, tileWidth, tileHeight);

            // Load tilesets
            foreach (XElement tilesetElement in mapElement.Elements("tileset"))
            {
                Tileset tileset = LoadTileset(tilesetElement, tmxPath, assetManager);
                if (tileset != null)
                {
                    tileMap.AddTileset(tileset);
                }
            }

            // Load layers
            foreach (XElement layerElement in mapElement.Elements("layer"))
            {
                TileLayer layer = LoadLayer(layerElement, tileMap);
                if (layer != null)
                {
                    tileMap.AddLayer(layer);
                }
            }

            // Build vertex arrays for rendering
            tileMap.BuildAllLayers();

            $"Loaded TMX map: {tmxPath} ({width}x{height}, tile size: {tileWidth}x{tileHeight})".Info(source: "TmxMapLoader");
            return tileMap;
        }
        catch (System.Exception ex)
        {
            $"Failed to load TMX file {tmxPath}: {ex.Message}".Error(source: "TmxMapLoader");
            return null;
        }
    }

    /// <summary>
    /// Loads a tileset from a TMX tileset XML element.
    /// </summary>
    /// <param name="tilesetElement">The XML element containing tileset data from the TMX file.</param>
    /// <param name="tmxPath">The file path of the parent TMX file, used to resolve relative image paths.</param>
    /// <param name="assetManager">The asset manager used to load the tileset texture.</param>
    /// <returns>
    /// A fully initialized <see cref="Tileset"/> instance with texture and tile properties loaded,
    /// or <c>null</c> if loading fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method extracts the following tileset data:
    /// </para>
    /// <list type="bullet">
    /// <item>Basic properties: FirstGid, Name, TileWidth, TileHeight, TileCount, Columns</item>
    /// <item>Layout properties: Spacing, Margin</item>
    /// <item>Image path and texture loading via <see cref="AssetManager"/></item>
    /// <item>Per-tile custom properties (e.g., collision flags)</item>
    /// </list>
    /// <para>
    /// Image paths in the tileset are resolved relative to the TMX file directory and converted
    /// to absolute paths before loading. Failed texture loads generate warnings but do not prevent
    /// tileset creation.
    /// </para>
    /// </remarks>
    private static Tileset LoadTileset(XElement tilesetElement, System.String tmxPath, AssetManager assetManager)
    {
        try
        {
            Tileset tileset = new()
            {
                FirstGid = System.Int16.Parse(tilesetElement.Attribute("firstgid")?.Value ?? "1"),
                Name = tilesetElement.Attribute("name")?.Value ?? "Unnamed",
                TileWidth = System.Int16.Parse(tilesetElement.Attribute("tilewidth")?.Value ?? "0"),
                TileHeight = System.Int16.Parse(tilesetElement.Attribute("tileheight")?.Value ?? "0"),
                TileCount = System.Int16.Parse(tilesetElement.Attribute("tilecount")?.Value ?? "0"),
                Columns = System.Int16.Parse(tilesetElement.Attribute("columns")?.Value ?? "0"),
                Spacing = System.Int16.Parse(tilesetElement.Attribute("spacing")?.Value ?? "0"),
                Margin = System.Int16.Parse(tilesetElement.Attribute("margin")?.Value ?? "0")
            };

            // Load image
            XElement imageElement = tilesetElement.Element("image");
            if (imageElement != null)
            {
                System.String imagePath = imageElement.Attribute("source")?.Value;
                if (!System.String.IsNullOrEmpty(imagePath))
                {
                    // Make path relative to TMX file
                    System.String tmxDir = System.IO.Path.GetDirectoryName(tmxPath);
                    System.String fullImagePath = System.IO.Path.Combine(tmxDir, imagePath);
                    fullImagePath = System.IO.Path.GetFullPath(fullImagePath);

                    tileset.ImagePath = fullImagePath;
                    tileset.Texture = assetManager.LoadTexture(fullImagePath);

                    if (tileset.Texture == null)
                    {
                        $"Failed to load tileset texture: {fullImagePath}".Warn(source: "TmxMapLoader");
                    }
                }
            }

            // Load tile properties (collision, custom properties)
            foreach (XElement tileElement in tilesetElement.Elements("tile"))
            {
                System.Int32 tileId = System.Int32.Parse(tileElement.Attribute("id")?.Value ?? "-1");
                if (tileId < 0)
                {
                    continue;
                }

                System.Collections.Generic.Dictionary<System.String, System.String> props = [];

                XElement propertiesElement = tileElement.Element("properties");
                if (propertiesElement != null)
                {
                    foreach (XElement propElement in propertiesElement.Elements("property"))
                    {
                        System.String propName = propElement.Attribute("name")?.Value;
                        System.String propValue = propElement.Attribute("value")?.Value;

                        if (!System.String.IsNullOrEmpty(propName))
                        {
                            props[propName] = propValue ?? System.String.Empty;
                        }
                    }
                }

                tileset.TileProperties[tileId] = props;
            }

            return tileset;
        }
        catch (System.Exception ex)
        {
            $"Failed to load tileset: {ex.Message}".Error(source: "TmxMapLoader");
            return null;
        }
    }

    /// <summary>
    /// Loads a tile layer from a TMX layer XML element.
    /// </summary>
    /// <param name="layerElement">The XML element containing layer data from the TMX file.</param>
    /// <param name="tileMap">The parent <see cref="TileMap"/> used to resolve tilesets for GID lookup.</param>
    /// <returns>
    /// A fully initialized <see cref="TileLayer"/> instance with all tiles populated,
    /// or <c>null</c> if loading fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method extracts layer metadata (name, visibility, opacity) and custom properties,
    /// then delegates to either <see cref="LoadCsvData"/> or <see cref="LoadXmlData"/> depending
    /// on the data encoding format specified in the layer's data element.
    /// </para>
    /// <para>
    /// Supported encoding formats:
    /// </para>
    /// <list type="bullet">
    /// <item><c>csv</c>: Comma-separated values (efficient, human-readable)</item>
    /// <item>XML tile elements: Default format with explicit &lt;tile&gt; elements</item>
    /// </list>
    /// </remarks>
    private static TileLayer LoadLayer(XElement layerElement, TileMap tileMap)
    {
        try
        {
            System.String name = layerElement.Attribute("name")?.Value ?? "Unnamed";
            System.Int32 width = System.Int32.Parse(layerElement.Attribute("width")?.Value ?? "0");
            System.Int32 height = System.Int32.Parse(layerElement.Attribute("height")?.Value ?? "0");
            System.Boolean visible = System.Int32.Parse(layerElement.Attribute("visible")?.Value ?? "1") == 1;
            System.Single opacity = System.Single.Parse(layerElement.Attribute("opacity")?.Value ?? "1");

            TileLayer layer = new(width, height)
            {
                Name = name,
                Visible = visible,
                Opacity = opacity
            };

            // Load layer properties
            XElement propertiesElement = layerElement.Element("properties");
            if (propertiesElement != null)
            {
                foreach (XElement propElement in propertiesElement.Elements("property"))
                {
                    System.String propName = propElement.Attribute("name")?.Value;
                    System.String propValue = propElement.Attribute("value")?.Value;

                    if (!System.String.IsNullOrEmpty(propName))
                    {
                        layer.Properties[propName] = propValue ?? System.String.Empty;
                    }
                }
            }

            // Load tile data
            XElement dataElement = layerElement.Element("data");
            if (dataElement != null)
            {
                System.String encoding = dataElement.Attribute("encoding")?.Value;

                if (encoding == "csv")
                {
                    LoadCsvData(dataElement, layer, tileMap);
                }
                else
                {
                    // Default: XML tile elements
                    LoadXmlData(dataElement, layer, tileMap);
                }
            }

            return layer;
        }
        catch (System.Exception ex)
        {
            $"Failed to load layer: {ex.Message}".Error(source: "TmxMapLoader");
            return null;
        }
    }

    /// <summary>
    /// Loads tile data from a CSV-encoded data element and populates the layer.
    /// </summary>
    /// <param name="dataElement">The XML data element containing CSV-encoded tile GIDs.</param>
    /// <param name="layer">The target <see cref="TileLayer"/> to populate with tiles.</param>
    /// <param name="tileMap">The parent <see cref="TileMap"/> used to resolve tilesets and calculate world positions.</param>
    /// <remarks>
    /// <para>
    /// CSV encoding stores tile GIDs as comma-separated integers in row-major order.
    /// This format is more compact than XML tile elements and faster to parse.
    /// </para>
    /// <para>
    /// For each non-zero GID, this method:
    /// </para>
    /// <list type="number">
    /// <item>Resolves the appropriate tileset using <see cref="TileMap.GetTilesetForGid"/>.</item>
    /// <item>Converts the global ID to a local tileset ID via <see cref="Tileset.GidToLocalId"/>.</item>
    /// <item>Creates a <see cref="Tile"/> with texture coordinates from <see cref="Tileset.GetTileRect"/>.</item>
    /// <item>Applies tile properties, including collision detection flags.</item>
    /// <item>Inserts the tile into the layer at the correct position.</item>
    /// </list>
    /// <para>
    /// GID value 0 represents an empty tile and is skipped. Collision properties are detected
    /// by checking for a "collision" property with value "true" or "1".
    /// </para>
    /// </remarks>
    private static void LoadCsvData(XElement dataElement, TileLayer layer, TileMap tileMap)
    {
        System.String csvData = dataElement.Value.Trim();
        System.String[] values = csvData.Split([',', '\n', '\r'], System.StringSplitOptions.RemoveEmptyEntries);

        System.Int32 index = 0;
        for (System.Int32 y = 0; y < layer.Height; y++)
        {
            for (System.Int32 x = 0; x < layer.Width; x++)
            {
                if (index >= values.Length)
                {
                    break;
                }

                System.Int32 gid = System.Int32.Parse(values[index].Trim());
                index++;

                if (gid == 0)
                {
                    continue; // Empty tile
                }

                Tileset tileset = tileMap.GetTilesetForGid(gid);
                if (tileset == null)
                {
                    continue;
                }

                System.Int32 localId = tileset.GidToLocalId(gid);
                if (localId < 0)
                {
                    continue;
                }

                Tile tile = new()
                {
                    Gid = (System.Int16)gid,
                    LocalId = (System.Int16)localId,
                    X = (System.Int16)x,
                    Y = (System.Int16)y,
                    WorldPosition = new SFML.System.Vector2f(x * tileMap.TileWidth, y * tileMap.TileHeight),
                    TextureRect = tileset.GetTileRect(localId)
                };

                // Check collision property
                if (tileset.TileProperties.TryGetValue(localId, out var props))
                {
                    if (props.TryGetValue("collision", out System.String collisionValue))
                    {
                        tile.IsCollidable = collisionValue is "true" or "1";
                    }

                    tile.Properties = new System.Collections.Generic.Dictionary<System.String, System.String>(props);
                }

                layer.SetTile(x, y, tile);
            }
        }
    }

    /// <summary>
    /// Loads tile data from XML tile elements and populates the layer.
    /// </summary>
    /// <param name="dataElement">The XML data element containing child &lt;tile&gt; elements with GID attributes.</param>
    /// <param name="layer">The target <see cref="TileLayer"/> to populate with tiles.</param>
    /// <param name="tileMap">The parent <see cref="TileMap"/> used to resolve tilesets and calculate world positions.</param>
    /// <remarks>
    /// <para>
    /// XML encoding stores each tile as a separate &lt;tile gid="..."&gt; element in row-major order.
    /// This is the default Tiled export format when no encoding is specified.
    /// </para>
    /// <para>
    /// The tile creation process is identical to <see cref="LoadCsvData"/>, but parsing uses
    /// XML element traversal instead of CSV string splitting. This method handles the same
    /// GID resolution, tileset lookup, and property application logic.
    /// </para>
    /// <para>
    /// GID value 0 represents an empty tile and is skipped. Collision properties are detected
    /// by checking for a "collision" property with value "true" or "1".
    /// </para>
    /// </remarks>
    private static void LoadXmlData(XElement dataElement, TileLayer layer, TileMap tileMap)
    {
        System.Collections.Generic.List<XElement> tiles = [.. dataElement.Elements("tile")];
        System.Int32 index = 0;

        for (System.Int32 y = 0; y < layer.Height; y++)
        {
            for (System.Int32 x = 0; x < layer.Width; x++)
            {
                if (index >= tiles.Count)
                {
                    break;
                }

                XElement tileElement = tiles[index];
                System.Int32 gid = System.Int32.Parse(tileElement.Attribute("gid")?.Value ?? "0");
                index++;

                if (gid == 0)
                {
                    continue;
                }

                Tileset tileset = tileMap.GetTilesetForGid(gid);
                if (tileset == null)
                {
                    continue;
                }

                System.Int32 localId = tileset.GidToLocalId(gid);
                if (localId < 0)
                {
                    continue;
                }

                Tile tile = new()
                {
                    Gid = (System.Int16)gid,
                    LocalId = (System.Int16)localId,
                    X = (System.Int16)x,
                    Y = (System.Int16)y,
                    WorldPosition = new SFML.System.Vector2f(x * tileMap.TileWidth, y * tileMap.TileHeight),
                    TextureRect = tileset.GetTileRect(localId)
                };

                if (tileset.TileProperties.TryGetValue(localId, out var props))
                {
                    if (props.TryGetValue("collision", out System.String collisionValue))
                    {
                        tile.IsCollidable = collisionValue is "true" or "1";
                    }

                    tile.Properties = new System.Collections.Generic.Dictionary<System.String, System.String>(props);
                }

                layer.SetTile(x, y, tile);
            }
        }
    }
}