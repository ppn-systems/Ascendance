// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps;
using Ascendance.Maps.Abstractions;
using Ascendance.Maps.Core;
using Ascendance.Maps.Layers;
using Ascendance.Maps.Objects;
using Ascendance.Maps.Tilesets;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Rendering;

/// <summary>
/// Debug version of the TMX Map Renderer with extensive logging.
/// </summary>
/// <remarks>
/// This renderer is intended for diagnostics, providing visibility into map and tileset
/// metadata, as well as tile caching behavior. It does not optimize for production use.
/// </remarks>
/// <seealso cref="TmxMap"/>
/// <seealso cref="TmxTileset"/>
public class TmxMapRenderer : RenderObject
{
    #region Fields

    /// <summary>
    /// Holds draw data for currently visible tiles based on culling and viewport calculations.
    /// </summary>
    private readonly System.Collections.Generic.List<TileDrawData> _visibleTiles;

    /// <summary>
    /// Caches textures by a string key (e.g., tileset source path) to avoid redundant loads.
    /// </summary>
    private readonly System.Collections.Generic.Dictionary<System.String, Texture> _textureCache;

    /// <summary>
    /// Maps global tile IDs (GIDs) to their originating TMX tileset for quick lookup.
    /// </summary>
    private readonly System.Collections.Generic.Dictionary<System.Int32, TmxTileset> _gidToTilesetCache;

    /// <summary>
    /// Indicates whether internal caches must be rebuilt before the next draw.
    /// </summary>
    private System.Boolean _cacheDirty;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the TMX map instance to be rendered.
    /// </summary>
    /// <value>
    /// A non-null <see cref="TmxMap"/> reference provided at construction.
    /// </value>
    public TmxMap Map { get; }

    /// <summary>
    /// Gets or sets a value indicating whether viewport culling is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, only tiles visible within the current viewport will be considered for drawing,
    /// which can improve performance on large maps.
    /// </remarks>
    public System.Boolean UseViewportCulling { get; set; } = false;

    /// <summary>
    /// Gets or sets the render offset applied to the map in screen space.
    /// </summary>
    /// <remarks>
    /// Use this to pan or shift the map rendering position without modifying world coordinates.
    /// The unit is pixels in the render target coordinate system.
    /// </remarks>
    public Vector2f MapOffset { get; set; } = new Vector2f(0, 0);

    /// <summary>
    /// Gets the number of tiles currently considered visible and queued for rendering.
    /// </summary>
    public System.Int32 VisibleTileCount => _visibleTiles.Count;

    #endregion Properties

    #region Construction

    /// <summary>
    /// Initializes a new instance of the <see cref="TmxMapRenderer"/> class.
    /// </summary>
    /// <param name="map">The TMX map instance to render. Must not be <see langword="null"/>.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="map"/> is <see langword="null"/>.
    /// </exception>
    public TmxMapRenderer(TmxMap map)
    {
        System.ArgumentNullException.ThrowIfNull(map);
        this.Map = map;

        _cacheDirty = true;
        _textureCache = [];
        _visibleTiles = [];
        _gidToTilesetCache = [];

        DEBUG_PRINT_MAP_INFO();
        INITIALIZE_TILESETS();
        INITIALIZE_GID_CACHE();
        REBUILD_TILE_CACHE();
    }

    #endregion Construction

    #region Debug Methods

    private void DEBUG_PRINT_MAP_INFO()
    {
        System.Console.WriteLine("=== TMX MAP DEBUG INFO ===");
        System.Console.WriteLine($"Map Size: {this.Map.Width}x{this.Map.Height} tiles");
        System.Console.WriteLine($"Tile Size: {this.Map.TileWidth}x{this.Map.TileHeight} pixels");
        System.Console.WriteLine($"Total Layers: {this.Map.Layers.Count}");
        System.Console.WriteLine($"Tile Layers: {this.Map.TileLayers.Count}");
        System.Console.WriteLine($"Object Groups: {this.Map.ObjectGroups.Count}");
        System.Console.WriteLine($"Image Layers: {this.Map.ImageLayers.Count}");
        System.Console.WriteLine($"Groups: {this.Map.Groups.Count}");
        System.Console.WriteLine($"Tilesets: {this.Map.Tilesets.Count}");

        // Tileset details
        for (System.Int32 i = 0; i < this.Map.Tilesets.Count; i++)
        {
            var ts = this.Map.Tilesets[i];
            System.Console.WriteLine($"Tileset {i}: {ts.Name}");
            System.Console.WriteLine($"  FirstGid: {ts.FirstGid}");
            System.Console.WriteLine($"  TileCount: {ts.TileCount}");
            System.Console.WriteLine($"  Image: {ts.Image?.Source ?? "No Image"}");
            System.Console.WriteLine($"  Size: {ts.TileWidth}x{ts.TileHeight}");
        }

        // Tile layer info
        for (System.Int32 i = 0; i < this.Map.TileLayers.Count; i++)
        {
            var layer = this.Map.TileLayers[i];
            System.Console.WriteLine($"Tile Layer {i}: {layer.Name}");
            System.Console.WriteLine($"  Visible: {layer.Visible}");
            System.Console.WriteLine($"  Tile Count: {layer.Tiles.Count}");
            System.Console.WriteLine($"  Opacity: {layer.Opacity}");

            // Show a few tiles
            System.Int32 tileCount = System.Math.Min(5, layer.Tiles.Count);
            for (System.Int32 j = 0; j < tileCount; j++)
            {
                var tile = layer.Tiles[j];
                System.Console.WriteLine($"    Tile {j}: GID={tile.Gid}, Pos=({tile.X},{tile.Y})");
            }
        }

        System.Console.WriteLine("========================");
    }

    #endregion Debug Methods

    #region Overrides

    public override void Draw(RenderTarget target)
    {
        if (!base.IsVisible)
        {
            return;
        }

        if (_cacheDirty)
        {
            REBUILD_TILE_CACHE();
        }

        // Render layers
        foreach (ITmxLayer layer in this.Map.Layers)
        {
            // Root call uses default parent opacity and offset
            RENDER_SINGLE_LAYER(target, layer);
        }
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => throw new System.NotImplementedException();

    #endregion Overrides

    #region Private Methods

    private void INITIALIZE_TILESETS()
    {
        foreach (TmxTileset tileset in this.Map.Tilesets)
        {
            if (System.String.IsNullOrEmpty(tileset.Image?.Source))
            {
                continue;
            }

            try
            {
                System.String imagePath = tileset.Image.Source;

                // Try multiple path candidates
                System.String[] pathsToTry = [
                    imagePath,
                    System.IO.Path.Combine(this.Map.TmxDirectory ?? "", imagePath),
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), imagePath),
                    System.IO.Path.GetFullPath(imagePath)
                ];

                System.Boolean loaded = false;
                foreach (System.String pathToTry in pathsToTry)
                {
                    if (System.IO.File.Exists(pathToTry))
                    {
                        Texture texture = new(pathToTry);
                        _textureCache[tileset.Image.Source] = texture;
                        loaded = true;
                        break;
                    }
                }

                if (!loaded)
                {
                    // Create a blue placeholder texture
                    Texture placeholderTexture = new(32, 32);
                    System.Byte[] placeholderPixels = new System.Byte[32 * 32 * 4];
                    for (System.Int32 i = 0; i < placeholderPixels.Length; i += 4)
                    {
                        placeholderPixels[i] = 0;       // R
                        placeholderPixels[i + 1] = 0;   // G
                        placeholderPixels[i + 2] = 255; // B
                        placeholderPixels[i + 3] = 255; // A
                    }

                    placeholderTexture.Update(placeholderPixels);
                    _textureCache[tileset.Image.Source] = placeholderTexture;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Exception loading texture {tileset.Image.Source}: {ex.Message}");
            }
        }
    }

    private void INITIALIZE_GID_CACHE()
    {
        foreach (TmxTileset tileset in this.Map.Tilesets)
        {
            System.Int32 tileCount = tileset.TileCount ?? 100; // Default fallback

            if (_textureCache.TryGetValue(tileset.Image?.Source ?? "", out var texture))
            {
                // Compute tile count from texture size when possible
                System.Int32 tilesPerRow = tileset.Columns ?? (System.Int32)(texture.Size.X / (System.UInt32)tileset.TileWidth);
                System.Int32 tilesPerCol = (System.Int32)(texture.Size.Y / (System.UInt32)tileset.TileHeight);
                tileCount = tilesPerRow * tilesPerCol;
            }

            System.Int32 maxGid = tileset.FirstGid + tileCount - 1;

            for (System.Int32 gid = tileset.FirstGid; gid <= maxGid; gid++)
            {
                _gidToTilesetCache[gid] = tileset;
            }
        }
    }

    private void REBUILD_TILE_CACHE()
    {
        _visibleTiles.Clear();

        foreach (TmxLayer tileLayer in this.Map.TileLayers)
        {
            if (!tileLayer.Visible)
            {
                continue;
            }

            foreach (TmxLayerTile tile in tileLayer.Tiles)
            {
                if (tile.Gid == 0)
                {
                    continue;
                }

                System.Single tileWorldX = (tile.X * this.Map.TileWidth) + MapOffset.X + (System.Single)(tileLayer.OffsetX ?? 0.0);
                System.Single tileWorldY = (tile.Y * this.Map.TileHeight) + MapOffset.Y + (System.Single)(tileLayer.OffsetY ?? 0.0);

                if (_gidToTilesetCache.TryGetValue(tile.Gid, out TmxTileset tileset))
                {
                    TileDrawData? data = CALCULATE_TILE_DRAW_DATA(tile, tileset, tileWorldX, tileWorldY, tileLayer);

                    if (data.HasValue)
                    {
                        _visibleTiles.Add(data.Value);
                    }
                }
            }
        }

        _cacheDirty = false;
    }

    private TileDrawData? CALCULATE_TILE_DRAW_DATA(TmxLayerTile tile, TmxTileset tileset, System.Single worldX, System.Single worldY, TmxLayer ownerLayer)
    {
        if (!_textureCache.TryGetValue(tileset.Image?.Source ?? "", out var texture))
        {
            return null;
        }

        System.Int32 localId = tile.Gid - tileset.FirstGid;

        // Calculate source rectangle in tileset
        System.Int32 tilesPerRow = tileset.Columns ?? (System.Int32)(texture.Size.X / (System.UInt32)tileset.TileWidth);
        System.Int32 srcX = localId % tilesPerRow * tileset.TileWidth;
        System.Int32 srcY = localId / tilesPerRow * tileset.TileHeight;

        IntRect sourceRect = new(srcX, srcY, tileset.TileWidth, tileset.TileHeight);
        Vector2f position = new(worldX, worldY);

        return new TileDrawData
        {
            Texture = texture,
            SourceRect = sourceRect,
            Position = position,
            HorizontalFlip = tile.HorizontalFlip,
            VerticalFlip = tile.VerticalFlip,
            DiagonalFlip = tile.DiagonalFlip,
            Layer = ownerLayer
        };
    }

    /// <summary>
    /// Render one layer, dispatching by the concrete layer type.
    /// Handles tile layers, image layers, object groups, and group layers recursively.
    /// </summary>
    /// <param name="target">Render target.</param>
    /// <param name="layer">Layer to render.</param>
    private void RENDER_SINGLE_LAYER(RenderTarget target, ITmxLayer layer)
    {
        // Dispatch to specialized renderers. For groups, propagate parent opacity/offset.
        switch (layer)
        {
            case TmxLayer tileLayer:
                RENDER_TILE_LAYER(target, tileLayer, parentOpacity: 1.0, parentOffsetX: 0.0, parentOffsetY: 0.0);
                break;

            case TmxObjectGroup objGroup:
                RENDER_OBJECT_GROUP(target, objGroup, parentOpacity: 1.0, parentOffsetX: 0.0, parentOffsetY: 0.0);
                break;

            case TmxImageLayer imgLayer:
                RENDER_IMAGE_LAYER(target, imgLayer, parentOpacity: 1.0, parentOffsetX: 0.0, parentOffsetY: 0.0);
                break;

            case TmxGroup group:
                RENDER_GROUP(target, group, parentOpacity: 1.0, parentOffsetX: 0.0, parentOffsetY: 0.0);
                break;

            default:
                System.Console.WriteLine($"Unknown layer type: {layer.GetType().Name}");
                break;
        }
    }

    /// <summary>
    /// Render a tile layer. Only draws tiles cached for the given layer.
    /// Applies opacity and offsets (including parent offsets).
    /// </summary>
    private void RENDER_TILE_LAYER(RenderTarget target, TmxLayer layer, System.Double parentOpacity, System.Double parentOffsetX, System.Double parentOffsetY)
    {
        if (!layer.Visible)
        {
            return;
        }

        // Combine opacity: parent * layer
        System.Double opacity = System.Math.Clamp(parentOpacity * layer.Opacity, 0.0, 1.0);
        System.Byte alpha = (System.Byte)System.Math.Clamp((System.Int32)(opacity * 255.0), 0, 255);

        // Draw only the tiles belonging to this layer
        foreach (TileDrawData tileData in _visibleTiles)
        {
            if (!System.Object.ReferenceEquals(tileData.Layer, layer))
            {
                continue;
            }

            Sprite sprite = new(tileData.Texture, tileData.SourceRect)
            {
                // Combine offsets: cached position already includes MapOffset + layer offset;
                // add parent offsets if any (e.g., group offsets).
                Position = new Vector2f(
                    tileData.Position.X + (System.Single)parentOffsetX,
                    tileData.Position.Y + (System.Single)parentOffsetY
                )
            };

            // Apply flips
            Vector2f scale = new(1f, 1f);
            Vector2f origin = new(0f, 0f);

            if (tileData.HorizontalFlip)
            {
                scale.X = -1f;
                origin.X = tileData.SourceRect.Width;
            }

            if (tileData.VerticalFlip)
            {
                scale.Y = -1f;
                origin.Y = tileData.SourceRect.Height;
            }

            // TODO: Diagonal flip support (requires rotation/shear or swap axes)

            sprite.Scale = scale;
            sprite.Origin = origin;

            // Apply opacity via vertex color alpha
            sprite.Color = new Color(255, 255, 255, alpha);

            target.Draw(sprite);
        }
    }

    /// <summary>
    /// Render an image layer (a single image with optional offsets and opacity).
    /// </summary>
    private void RENDER_IMAGE_LAYER(RenderTarget target, TmxImageLayer imgLayer, System.Double parentOpacity, System.Double parentOffsetX, System.Double parentOffsetY)
    {
        if (!imgLayer.Visible || imgLayer.Image is null)
        {
            return;
        }

        Texture texture = GET_OR_LOAD_TEXTURE(imgLayer.Image);
        if (texture is null)
        {
            System.Console.WriteLine($"Image layer '{imgLayer.Name}' has no loadable texture: {imgLayer.Image.Source ?? "(embedded/unknown)"}");
            return;
        }

        Sprite sprite = new(texture);

        // Position: MapOffset + parent offsets + layer offsets
        System.Single posX = MapOffset.X + (System.Single)parentOffsetX + (System.Single)(imgLayer.OffsetX ?? 0.0);
        System.Single posY = MapOffset.Y + (System.Single)parentOffsetY + (System.Single)(imgLayer.OffsetY ?? 0.0);
        sprite.Position = new Vector2f(posX, posY);

        // Apply opacity
        System.Double opacity = System.Math.Clamp(parentOpacity * imgLayer.Opacity, 0.0, 1.0);
        System.Byte alpha = (System.Byte)System.Math.Clamp((System.Int32)(opacity * 255.0), 0, 255);
        sprite.Color = new Color(255, 255, 255, alpha);

        target.Draw(sprite);
    }

    /// <summary>
    /// Render an object group. Currently renders only tile objects (with GIDs).
    /// Other object types are logged for future implementation.
    /// </summary>
    private void RENDER_OBJECT_GROUP(RenderTarget target, TmxObjectGroup objGroup, System.Double parentOpacity, System.Double parentOffsetX, System.Double parentOffsetY)
    {
        if (!objGroup.Visible)
        {
            return;
        }

        System.Double opacity = System.Math.Clamp(parentOpacity * objGroup.Opacity, 0.0, 1.0);
        System.Byte alpha = (System.Byte)System.Math.Clamp((System.Int32)(opacity * 255.0), 0, 255);

        foreach (TmxObject obj in objGroup.Objects)
        {
            // Render only tile objects for now
            if (obj.Tile is null || obj.Tile.Gid == 0)
            {
                // TODO: render ellipse, polygon, polyline, text (not implemented)
                System.Console.WriteLine($"[ObjectGroup:{objGroup.Name}] Skipping non-tile object Id={obj.Id}, Type={obj.ObjectType}");
                continue;
            }

            if (!_gidToTilesetCache.TryGetValue(obj.Tile.Gid, out var tileset))
            {
                System.Console.WriteLine($"[ObjectGroup:{objGroup.Name}] Unknown tileset for GID={obj.Tile.Gid}");
                continue;
            }

            if (!_textureCache.TryGetValue(tileset.Image?.Source ?? "", out var texture))
            {
                System.Console.WriteLine($"[ObjectGroup:{objGroup.Name}] Texture missing for tileset image '{tileset.Image?.Source}'");
                continue;
            }

            // Compute source rect like tile layers
            System.Int32 localId = obj.Tile.Gid - tileset.FirstGid;
            System.Int32 tilesPerRow = tileset.Columns ?? (System.Int32)(texture.Size.X / (System.UInt32)tileset.TileWidth);
            System.Int32 srcX = localId % tilesPerRow * tileset.TileWidth;
            System.Int32 srcY = localId / tilesPerRow * tileset.TileHeight;
            IntRect sourceRect = new(srcX, srcY, tileset.TileWidth, tileset.TileHeight);

            Sprite sprite = new(texture, sourceRect);

            // Position: objects use pixel coordinates (X,Y). Apply MapOffset + parent offsets + group offsets.
            System.Single totalOffsetX = MapOffset.X + (System.Single)parentOffsetX + (System.Single)(objGroup.OffsetX ?? 0.0);
            System.Single totalOffsetY = MapOffset.Y + (System.Single)parentOffsetY + (System.Single)(objGroup.OffsetY ?? 0.0);
            sprite.Position = new Vector2f((System.Single)obj.X + totalOffsetX, (System.Single)obj.Y + totalOffsetY);

            // Flips
            Vector2f scale = new(1f, 1f);
            Vector2f origin = new(0f, 0f);
            if (obj.Tile.HorizontalFlip)
            {
                scale.X = -1f;
                origin.X = sourceRect.Width;
            }
            if (obj.Tile.VerticalFlip)
            {
                scale.Y = -1f;
                origin.Y = sourceRect.Height;
            }
            // TODO: Diagonal flip support

            sprite.Scale = scale;
            sprite.Origin = origin;

            // Apply opacity
            sprite.Color = new Color(255, 255, 255, alpha);

            target.Draw(sprite);
        }
    }

    /// <summary>
    /// Render a group layer recursively, propagating opacity and offset to child layers.
    /// </summary>
    private void RENDER_GROUP(RenderTarget target, TmxGroup group, System.Double parentOpacity, System.Double parentOffsetX, System.Double parentOffsetY)
    {
        if (!group.Visible)
        {
            return;
        }

        // Combined opacity and offsets
        System.Double combinedOpacity = System.Math.Clamp(parentOpacity * group.Opacity, 0.0, 1.0);
        System.Double combinedOffsetX = parentOffsetX + (group.OffsetX ?? 0.0);
        System.Double combinedOffsetY = parentOffsetY + (group.OffsetY ?? 0.0);

        foreach (ITmxLayer child in group.Layers)
        {
            switch (child)
            {
                case TmxLayer tileLayer:
                    RENDER_TILE_LAYER(target, tileLayer, combinedOpacity, combinedOffsetX, combinedOffsetY);
                    break;
                case TmxObjectGroup objGroup:
                    RENDER_OBJECT_GROUP(target, objGroup, combinedOpacity, combinedOffsetX, combinedOffsetY);
                    break;
                case TmxImageLayer imgLayer:
                    RENDER_IMAGE_LAYER(target, imgLayer, combinedOpacity, combinedOffsetX, combinedOffsetY);
                    break;
                case TmxGroup nested:
                    RENDER_GROUP(target, nested, combinedOpacity, combinedOffsetX, combinedOffsetY);
                    break;
                default:
                    System.Console.WriteLine($"Unexpected child layer type: {child.GetType().Name}");
                    break;
            }
        }
    }

    /// <summary>
    /// Try to get or load a texture for a given TMX image. Returns null when it cannot be loaded.
    /// </summary>
    private Texture GET_OR_LOAD_TEXTURE(TmxImage image)
    {
        if (image is null || System.String.IsNullOrWhiteSpace(image.Source))
        {
            return null;
        }

        // Return from cache if available
        if (_textureCache.TryGetValue(image.Source, out var cached))
        {
            return cached;
        }

        try
        {
            // Attempt several path variants
            System.String[] pathsToTry = [
                image.Source,
                System.IO.Path.Combine(this.Map.TmxDirectory ?? "", image.Source),
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), image.Source),
                System.IO.Path.GetFullPath(image.Source)
            ];

            foreach (System.String p in pathsToTry)
            {
                if (System.IO.File.Exists(p))
                {
                    Texture tex = new(p);
                    _textureCache[image.Source] = tex;
                    return tex;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Failed to load image texture '{image.Source}': {ex.Message}");
        }

        return null;
    }

    #endregion Private Methods

    #region Nested Types

    private struct TileDrawData
    {
        public TmxLayer Layer; // Owner layer reference, used to filter when rendering
        public Texture Texture;
        public Vector2f Position;
        public IntRect SourceRect;
        public System.Boolean VerticalFlip;
        public System.Boolean DiagonalFlip;
        public System.Boolean HorizontalFlip;
    }

    #endregion Nested Types
}