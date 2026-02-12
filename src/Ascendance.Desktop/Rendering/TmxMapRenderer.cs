// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps;
using Ascendance.Maps.Abstractions;
using Ascendance.Maps.Core;
using Ascendance.Maps.Layers;
using Ascendance.Maps.Tilesets;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Rendering;

/// <summary>
/// Debug version of TMX Map Renderer with extensive logging
/// </summary>
public class TmxMapRenderer : RenderObject
{

    #region Fields
    private readonly System.Collections.Generic.Dictionary<System.String, Texture> _textureCache;
    private readonly System.Collections.Generic.Dictionary<System.Int32, TmxTileset> _gidToTilesetCache;
    private readonly System.Collections.Generic.List<TileDrawData> _visibleTiles;

    private System.Boolean _useViewportCulling;
    private FloatRect _viewportBounds;
    private System.Boolean _cacheDirty;
    private Vector2f _mapOffset;

    // Test sprite để đảm bảo renderer hoạt động
    private readonly Sprite _testSprite;
    private readonly RectangleShape _debugRect;

    #endregion Fields

    #region Properties

    public TmxMap Map { get; }
    public System.Boolean UseViewportCulling { get; set; } = false;
    public Vector2f MapOffset { get; set; } = new Vector2f(0, 0);
    public System.Int32 VisibleTileCount => _visibleTiles.Count;

    #endregion Properties

    #region Construction

    public TmxMapRenderer(TmxMap map)
    {
        this.Map = map ?? throw new System.ArgumentNullException(nameof(map));

        _textureCache = [];
        _visibleTiles = [];
        _gidToTilesetCache = [];

        _cacheDirty = true;

        // Tạo test sprite màu đỏ để kiểm tra renderer
        Texture testTexture = new(64, 64);
        System.Byte[] pixels = new System.Byte[64 * 64 * 4];

        for (System.Int32 i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;     // R
            pixels[i + 1] = 0;   // G
            pixels[i + 2] = 0;   // B
            pixels[i + 3] = 255; // A
        }
        testTexture.Update(pixels);
        _testSprite = new Sprite(testTexture) { Position = new Vector2f(100, 100) };

        // Debug rectangle
        _debugRect = new RectangleShape(new Vector2f(100, 100))
        {
            OutlineThickness = 2f,
            FillColor = Color.Green,
            OutlineColor = Color.White,
            Position = new Vector2f(200, 200)
        };

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

        // In thông tin chi tiết về tilesets
        for (System.Int32 i = 0; i < this.Map.Tilesets.Count; i++)
        {
            var ts = this.Map.Tilesets[i];
            System.Console.WriteLine($"Tileset {i}: {ts.Name}");
            System.Console.WriteLine($"  FirstGid: {ts.FirstGid}");
            System.Console.WriteLine($"  TileCount: {ts.TileCount}");
            System.Console.WriteLine($"  Image: {ts.Image?.Source ?? "No Image"}");
            System.Console.WriteLine($"  Size: {ts.TileWidth}x{ts.TileHeight}");
        }

        // In thông tin về tile layers
        for (System.Int32 i = 0; i < this.Map.TileLayers.Count; i++)
        {
            var layer = this.Map.TileLayers[i];
            System.Console.WriteLine($"Tile Layer {i}: {layer.Name}");
            System.Console.WriteLine($"  Visible: {layer.Visible}");
            System.Console.WriteLine($"  Tile Count: {layer.Tiles.Count}");
            System.Console.WriteLine($"  Opacity: {layer.Opacity}");

            // In vài tile đầu tiên
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
        if (!IsVisible)
        {
            return;
        }

        target.Draw(_testSprite);
        target.Draw(_debugRect);

        if (_cacheDirty)
        {
            REBUILD_TILE_CACHE();
        }

        // Render layers
        System.Int32 layersRendered = 0;
        foreach (var layer in this.Map.Layers)
        {
            if (layer.Visible)
            {
                RENDER_SINGLE_LAYER(target, layer);
                layersRendered++;
            }
        }
    }

    protected override Drawable GetDrawable() => _testSprite;

    #endregion Overrides

    #region Private Methods

    private void INITIALIZE_TILESETS()
    {
        foreach (var tileset in this.Map.Tilesets)
        {
            if (System.String.IsNullOrEmpty(tileset.Image?.Source))
            {
                continue;
            }

            try
            {
                System.String imagePath = tileset.Image.Source;

                // Thử nhiều đường dẫn khác nhau
                System.String[] pathsToTry = [
                    imagePath, // Đường dẫn gốc
                    System.IO.Path.Combine(this.Map.TmxDirectory ?? "", imagePath), // Kết hợp với thư mục TMX
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), imagePath), // Thư mục hiện tại
                    System.IO.Path.GetFullPath(imagePath) // Full path
                ];

                System.Boolean loaded = false;
                foreach (var pathToTry in pathsToTry)
                {
                    if (System.IO.File.Exists(pathToTry))
                    {
                        var texture = new Texture(pathToTry);
                        _textureCache[tileset.Image.Source] = texture;
                        loaded = true;
                        break;
                    }
                }

                if (!loaded)
                {
                    // Tạo texture placeholder màu xanh
                    var placeholderTexture = new Texture(32, 32);
                    var placeholderPixels = new System.Byte[32 * 32 * 4];
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
        foreach (var tileset in this.Map.Tilesets)
        {
            System.Int32 tileCount = tileset.TileCount ?? 100; // Default fallback

            if (_textureCache.TryGetValue(tileset.Image?.Source ?? "", out var texture))
            {
                // Tính tile count từ texture size
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

        System.Int32 totalTilesProcessed = 0;
        System.Int32 emptyTiles = 0;
        System.Int32 validTiles = 0;

        foreach (var tileLayer in this.Map.TileLayers)
        {
            if (!tileLayer.Visible)
            {
                continue;
            }

            foreach (var tile in tileLayer.Tiles)
            {
                totalTilesProcessed++;

                if (tile.Gid == 0)
                {
                    emptyTiles++;
                    continue;
                }

                System.Single tileWorldX = (tile.X * this.Map.TileWidth) + MapOffset.X;
                System.Single tileWorldY = (tile.Y * this.Map.TileHeight) + MapOffset.Y;

                if (_gidToTilesetCache.TryGetValue(tile.Gid, out var tileset))
                {
                    var drawData = CALCULATE_TILE_DRAW_DATA(tile, tileset, tileWorldX, tileWorldY);
                    if (drawData.HasValue)
                    {
                        _visibleTiles.Add(drawData.Value);
                        validTiles++;
                    }
                }
            }
        }

        _cacheDirty = false;
    }

    private TileDrawData? CALCULATE_TILE_DRAW_DATA(TmxLayerTile tile, TmxTileset tileset, System.Single worldX, System.Single worldY)
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

        var sourceRect = new IntRect(srcX, srcY, tileset.TileWidth, tileset.TileHeight);
        var position = new Vector2f(worldX, worldY);

        return new TileDrawData
        {
            Texture = texture,
            SourceRect = sourceRect,
            Position = position,
            HorizontalFlip = tile.HorizontalFlip,
            VerticalFlip = tile.VerticalFlip,
            DiagonalFlip = tile.DiagonalFlip
        };
    }

    private void RENDER_SINGLE_LAYER(RenderTarget target, ITmxLayer layer)
    {
        switch (layer)
        {
            case TmxLayer tileLayer:
                RENDER_TILE_LAYER(target, tileLayer);
                break;
            case TmxObjectGroup objGroup:
                System.Console.WriteLine($"Rendering object group: {objGroup.Name}");
                break;
            case TmxImageLayer imgLayer:
                System.Console.WriteLine($"Rendering image layer: {imgLayer.Name}");
                break;
            case TmxGroup group:
                System.Console.WriteLine($"Rendering group: {group.Name}");
                break;
            default:
                System.Console.WriteLine($"Unknown layer type: {layer.GetType().Name}");
                break;
        }
    }

    private void RENDER_TILE_LAYER(RenderTarget target, TmxLayer layer)
    {
        System.Int32 renderedCount = 0;
        foreach (var tileData in _visibleTiles)
        {
            var sprite = new Sprite(tileData.Texture, tileData.SourceRect)
            {
                Position = tileData.Position
            };

            // Apply flips
            var scale = new Vector2f(1f, 1f);
            var origin = new Vector2f(0f, 0f);

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

            sprite.Scale = scale;
            sprite.Origin = origin;

            target.Draw(sprite);
            renderedCount++;
        }
    }

    #endregion Private Methods

    #region Nested Types

    private struct TileDrawData
    {
        public Texture Texture;
        public IntRect SourceRect;
        public Vector2f Position;
        public System.Boolean HorizontalFlip;
        public System.Boolean VerticalFlip;
        public System.Boolean DiagonalFlip;
    }

    #endregion Nested Types
}