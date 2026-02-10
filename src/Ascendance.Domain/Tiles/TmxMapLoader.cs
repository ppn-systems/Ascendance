// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Domain.Enums;
using Ascendance.Rendering.Managers;
using Nalix.Logging.Extensions;
using SFML.System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Ascendance.Domain.Tiles;

/// <summary>
/// Provides high-performance TMX (Tiled Map XML) file loading.
/// </summary>
public static class TmxMapLoader
{
    private const System.UInt32 TILE_GID_MASK = 0x1FFFFFFF;
    private const System.UInt32 FLIPPED_VERTICALLY_FLAG = 0x40000000;
    private const System.UInt32 FLIPPED_DIAGONALLY_FLAG = 0x20000000;
    private const System.UInt32 FLIPPED_HORIZONTALLY_FLAG = 0x80000000;

    #region Public API

    /// <summary>
    /// Loads a complete tile map from a TMX file.
    /// </summary>
    /// <param name="tmxPath">The path to the .tmx file.</param>
    /// <returns>A fully initialized <see cref="TileMap"/>, or <c>null</c> on failure.</returns>
    public static TileMap Load(System.String tmxPath)
    {
        if (System.String.IsNullOrWhiteSpace(tmxPath))
        {
            "TMX path cannot be null or empty.".Error(source: "TmxMapLoader");
            return null;
        }

        if (!System.IO.File.Exists(tmxPath))
        {
            $"TMX file not found: {tmxPath}".Error(source: "TmxMapLoader");
            return null;
        }

        try
        {
            XDocument doc = XDocument.Load(tmxPath);
            XElement mapElement = doc.Element("map");

            if (mapElement is null)
            {
                $"Invalid TMX file (missing <map> element): {tmxPath}".Error(source: "TmxMapLoader");
                return null;
            }

            // Parse map properties
            System.Int32 width = ParseInt(mapElement.Attribute("width"), 0);
            System.Int32 height = ParseInt(mapElement.Attribute("height"), 0);
            System.Int32 tileWidth = ParseInt(mapElement.Attribute("tilewidth"), 0);
            System.Int32 tileHeight = ParseInt(mapElement.Attribute("tileheight"), 0);

            if (width <= 0 || height <= 0 || tileWidth <= 0 || tileHeight <= 0)
            {
                $"Invalid map dimensions in TMX file: {tmxPath}".Error(source: "TmxMapLoader");
                return null;
            }

            TileMap tileMap = new(width, height, tileWidth, tileHeight);

            System.String baseDirectory = System.IO.Path.GetDirectoryName(tmxPath);

            // Load tilesets
            System.Int32 tilesetCount = 0;
            foreach (XElement tilesetElement in mapElement.Elements("tileset"))
            {
                Tileset tileset = LoadTileset(tilesetElement, baseDirectory);
                if (tileset is not null)
                {
                    tileMap.AddTileset(tileset);
                    tilesetCount++;
                }
            }

            if (tilesetCount == 0)
            {
                $"No valid tilesets found in TMX file: {tmxPath}".Warn(source: "TmxMapLoader");
            }

            // Load layers
            System.Int32 layerCount = 0;
            foreach (XElement layerElement in mapElement.Elements("layer"))
            {
                TileLayer layer = LoadLayer(layerElement, tileMap);
                if (layer is not null)
                {
                    tileMap.AddLayer(layer);
                    layerCount++;
                }
            }

            if (layerCount == 0)
            {
                $"No valid layers found in TMX file: {tmxPath}".Warn(source: "TmxMapLoader");
            }

            // Build vertex arrays for rendering
            tileMap.BuildAllLayers();

            $"Loaded TMX map: {tmxPath} ({width}x{height} tiles, {tileWidth}x{tileHeight}px, {layerCount} layers, {tilesetCount} tilesets)"
                .Info(source: "TmxMapLoader");

            return tileMap;
        }
        catch (System.Exception ex)
        {
            $"Failed to load TMX file {tmxPath}: {ex.Message}".Error(source: "TmxMapLoader");
            return null;
        }
    }

    #endregion Public API

    #region Tileset Loading

    private static Tileset LoadTileset(XElement tilesetElement, System.String baseDirectory)
    {
        try
        {
            // Check for external tileset reference
            XAttribute sourceAttr = tilesetElement.Attribute("source");
            if (sourceAttr is not null)
            {
                System.String tsxPath = System.IO.Path.Combine(baseDirectory, sourceAttr.Value);
                return LoadExternalTileset(tsxPath, tilesetElement);
            }

            return LoadEmbeddedTileset(tilesetElement, baseDirectory);
        }
        catch (System.Exception ex)
        {
            $"Failed to load tileset: {ex.Message}".Error(source: "TmxMapLoader");
            return null;
        }
    }

    private static Tileset LoadExternalTileset(System.String tsxPath, XElement mapTilesetElement)
    {
        if (!System.IO.File.Exists(tsxPath))
        {
            $"External tileset file not found: {tsxPath}".Error(source: "TmxMapLoader");
            return null;
        }

        XDocument tsxDoc = XDocument.Load(tsxPath);
        XElement tilesetElement = tsxDoc.Element("tileset");

        if (tilesetElement is null)
        {
            $"Invalid TSX file (missing <tileset> element): {tsxPath}".Error(source: "TmxMapLoader");
            return null;
        }

        System.String baseDirectory = System.IO.Path.GetDirectoryName(tsxPath);
        Tileset tileset = LoadEmbeddedTileset(tilesetElement, baseDirectory);

        // Override FirstGid from map's tileset reference
        tileset?.FirstGid = ParseInt(mapTilesetElement.Attribute("firstgid"), 1);

        return tileset;
    }

    private static Tileset LoadEmbeddedTileset(XElement tilesetElement, System.String baseDirectory)
    {
        Tileset tileset = new()
        {
            Name = tilesetElement.Attribute("name")?.Value ?? "Unnamed",
            FirstGid = ParseInt(tilesetElement.Attribute("firstgid"), 1),
            TileWidth = ParseInt(tilesetElement.Attribute("tilewidth"), 0),
            TileHeight = ParseInt(tilesetElement.Attribute("tileheight"), 0),
            TileCount = ParseInt(tilesetElement.Attribute("tilecount"), 0),
            Columns = ParseInt(tilesetElement.Attribute("columns"), 0),
            Spacing = ParseInt(tilesetElement.Attribute("spacing"), 0),
            Margin = ParseInt(tilesetElement.Attribute("margin"), 0)
        };

        // Load image
        XElement imageElement = tilesetElement.Element("image");
        if (imageElement is not null)
        {
            System.String imagePath = imageElement.Attribute("source")?.Value;
            if (!System.String.IsNullOrEmpty(imagePath))
            {
                System.String fullPath = System.IO.Path.Combine(baseDirectory, imagePath);
                tileset.ImagePath = fullPath;
                tileset.Texture = AssetManager.Instance.LoadTexture(fullPath);

                if (tileset.Texture is null)
                {
                    $"Failed to load tileset texture: {fullPath}".Warn(source: "TmxMapLoader");
                }
            }
        }

        // Load tile properties
        foreach (XElement tileElement in tilesetElement.Elements("tile"))
        {
            System.Int32 tileId = ParseInt(tileElement.Attribute("id"), -1);
            if (tileId < 0)
            {
                continue;
            }

            System.Collections.Generic.Dictionary<System.String, System.String> props = tileset.GetOrCreateTileProperties(tileId);

            LOAD_PROPERTIES(tileElement.Element("properties"), props);
        }

        return tileset;
    }

    #endregion Tileset Loading

    #region Layer Loading

    private static TileLayer LoadLayer(XElement layerElement, TileMap tileMap)
    {
        try
        {
            System.String name = layerElement.Attribute("name")?.Value ?? "Unnamed";
            System.Int32 width = ParseInt(layerElement.Attribute("width"), 0);
            System.Int32 height = ParseInt(layerElement.Attribute("height"), 0);
            System.Single opacity = ParseFloat(layerElement.Attribute("opacity"), 1.0f);
            System.Boolean visible = ParseInt(layerElement.Attribute("visible"), 1) != 0;

            if (width <= 0 || height <= 0)
            {
                $"Invalid layer dimensions for layer '{name}'".Error(source: "TmxMapLoader");
                return null;
            }

            TileLayer layer = new(width, height)
            {
                Name = name,
                Visible = visible,
                Opacity = opacity
            };

            // Load layer properties
            LOAD_PROPERTIES(layerElement.Element("properties"), layer.Properties);

            // Determine layer type from properties
            if (layer.Properties.TryGetValue("type", out System.String typeValue))
            {
                layer.LayerType = PARSE_LAYER_TYPE(typeValue);
            }
            else if (name.Contains("collision", System.StringComparison.OrdinalIgnoreCase))
            {
                layer.LayerType = TileLayerType.Collision;
            }
            else if (name.Contains("background", System.StringComparison.OrdinalIgnoreCase))
            {
                layer.LayerType = TileLayerType.Background;
            }
            else if (name.Contains("foreground", System.StringComparison.OrdinalIgnoreCase))
            {
                layer.LayerType = TileLayerType.Foreground;
            }

            // Load tile data
            XElement dataElement = layerElement.Element("data");
            if (dataElement is not null)
            {
                TileDataEncoding encoding = PARSE_ENCODING(dataElement.Attribute("encoding")?.Value);

                switch (encoding)
                {
                    case TileDataEncoding.Csv:
                        LoadCsvData(dataElement, layer, tileMap);
                        break;

                    case TileDataEncoding.Xml:
                        LoadXmlData(dataElement, layer, tileMap);
                        break;

                    case TileDataEncoding.Base64:
                    case TileDataEncoding.Base64Compressed:
                        $"Base64 encoding not yet supported for layer '{name}'".Warn(source: "TmxMapLoader");
                        break;
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

    #endregion Layer Loading

    #region Tile Data Loading

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void LoadCsvData(XElement dataElement, TileLayer layer, TileMap tileMap)
    {
        System.String csvData = dataElement.Value.Trim();
        System.String[] values = csvData.Split([',', '\n', '\r'], System.StringSplitOptions.RemoveEmptyEntries);

        System.Int32 expectedCount = layer.Width * layer.Height;
        if (values.Length < expectedCount)
        {
            $"CSV data has {values.Length} values but expected {expectedCount}".Warn(source: "TmxMapLoader");
        }

        System.Int32 index = 0;
        for (System.Int32 y = 0; y < layer.Height && index < values.Length; y++)
        {
            for (System.Int32 x = 0; x < layer.Width && index < values.Length; x++, index++)
            {
                if (!System.UInt32.TryParse(values[index].Trim(), out System.UInt32 rawGid))
                {
                    continue;
                }

                Tile tile = CreateTileFromGid(rawGid, x, y, tileMap);
                layer.SetTile(x, y, tile);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void LoadXmlData(XElement dataElement, TileLayer layer, TileMap tileMap)
    {
        System.Collections.Generic.IEnumerable<XElement> tileElements = dataElement.Elements("tile");

        System.Int32 index = 0;
        foreach (XElement tileElement in tileElements)
        {
            if (index >= layer.Width * layer.Height)
            {
                break;
            }

            System.Int32 x = index % layer.Width;
            System.Int32 y = index / layer.Width;
            index++;

            System.UInt32 rawGid = ParseUInt(tileElement.Attribute("gid"), 0);
            Tile tile = CreateTileFromGid(rawGid, x, y, tileMap);
            layer.SetTile(x, y, tile);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Tile CreateTileFromGid(System.UInt32 rawGid, System.Int32 x, System.Int32 y, TileMap tileMap)
    {
        // Extract flip flags
        System.Boolean flipH = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
        System.Boolean flipV = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
        System.Boolean flipD = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;

        // Clear flags to get actual GID
        System.Int32 gid = (System.Int32)(rawGid & TILE_GID_MASK);

        if (gid == 0)
        {
            return Tile.CreateEmpty(new Vector2f(x * tileMap.TileWidth, y * tileMap.TileHeight));
        }

        Tileset tileset = tileMap.GetTilesetForGid(gid);
        if (tileset is null)
        {
            return Tile.CreateEmpty(new Vector2f(x * tileMap.TileWidth, y * tileMap.TileHeight));
        }

        System.Int32 localId = tileset.GidToLocalId(gid);
        if (localId < 0)
        {
            return Tile.CreateEmpty(new Vector2f(x * tileMap.TileWidth, y * tileMap.TileHeight));
        }

        Vector2f worldPos = new(x * tileMap.TileWidth, y * tileMap.TileHeight);

        Tile tile = new(
            gid,
            localId,
            tileset.GetTileRect(localId),
            worldPos,
            false);

        // Apply flip flags
        if (flipH)
        {
            tile.SetFlippedHorizontally(true);
        }

        if (flipV)
        {
            tile.SetFlippedVertically(true);
        }

        if (flipD)
        {
            tile.SetFlippedDiagonally(true);
        }

        // Apply tile properties
        if (tileset.TileProperties.TryGetValue(localId, out var props))
        {
            if (props.TryGetValue("collision", out System.String collisionValue))
            {
                tile.SetCollidable(collisionValue is "true" or "1");
            }
        }

        return tile;
    }

    #endregion Tile Data Loading

    #region Helper Methods

    private static void LOAD_PROPERTIES(
        XElement propertiesElement,
        System.Collections.Generic.Dictionary<System.String, System.String> target)
    {
        if (propertiesElement is null)
        {
            return;
        }

        foreach (XElement propElement in propertiesElement.Elements("property"))
        {
            System.String propName = propElement.Attribute("name")?.Value;
            System.String propValue = propElement.Attribute("value")?.Value;

            if (!System.String.IsNullOrEmpty(propName))
            {
                target[propName] = propValue ?? System.String.Empty;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TileDataEncoding PARSE_ENCODING(System.String encodingString)
    {
        return System.String.IsNullOrWhiteSpace(encodingString)
            ? TileDataEncoding.Xml
            : encodingString.ToLowerInvariant() switch
            {
                "csv" => TileDataEncoding.Csv,
                "base64" => TileDataEncoding.Base64,
                _ => TileDataEncoding.Xml
            };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TileLayerType PARSE_LAYER_TYPE(System.String typeString)
    {
        return typeString?.ToLowerInvariant() switch
        {
            "background" => TileLayerType.Background,
            "ground" => TileLayerType.Ground,
            "decoration" => TileLayerType.Decoration,
            "collision" => TileLayerType.Collision,
            "foreground" => TileLayerType.Foreground,
            "overlay" => TileLayerType.Overlay,
            _ => TileLayerType.Ground
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static System.Int16 ParseInt(XAttribute attribute, System.Int16 defaultValue)
    {
        return attribute is not null && System.Int16.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out System.Int16 result)
            ? result
            : defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static System.UInt16 ParseUInt(XAttribute attribute, System.UInt16 defaultValue)
    {
        return attribute is not null && System.UInt16.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out System.UInt16 result)
            ? result
            : defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static System.Single ParseFloat(XAttribute attribute, System.Single defaultValue)
    {
        return attribute is not null && System.Single.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out System.Single result)
            ? result
            : defaultValue;
    }

    #endregion Helper Methods
}