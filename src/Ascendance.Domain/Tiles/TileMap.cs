// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Domain.Tiles;

/// <summary>
/// Represents a complete tile-based map that manages layers, tilesets, coordinate transformations, and rendering.
/// </summary>
/// <remarks>
/// High-performance tile map implementation with viewport culling, layer sorting, and efficient rendering.
/// </remarks>
public sealed class TileMap : RenderObject, System.IDisposable
{
    #region Fields

    private readonly System.Collections.Generic.List<TileLayer> _layers;
    private readonly System.Collections.Generic.List<Tileset> _tilesets;
    private readonly System.Collections.Generic.Dictionary<System.String, TileLayer> _layerCache;
    private System.Boolean _disposed;
    private System.Boolean _layersDirty = true;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the width of the map in tiles.
    /// </summary>
    public System.Int16 Width { get; }

    /// <summary>
    /// Gets the height of the map in tiles.
    /// </summary>
    public System.Int16 Height { get; }

    /// <summary>
    /// Gets the width of each tile in pixels.
    /// </summary>
    public System.Int16 TileWidth { get; }

    /// <summary>
    /// Gets the height of each tile in pixels.
    /// </summary>
    public System.Int16 TileHeight { get; }

    /// <summary>
    /// Gets the total width of the map in pixels.
    /// </summary>
    public System.Int32 PixelWidth
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => Width * TileWidth;
    }

    /// <summary>
    /// Gets the total height of the map in pixels.
    /// </summary>
    public System.Int32 PixelHeight
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => Height * TileHeight;
    }

    /// <summary>
    /// Gets the read-only list of all layers in the map.
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<TileLayer> Layers => _layers;

    /// <summary>
    /// Gets the read-only list of all tilesets used by this map.
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<Tileset> Tilesets => _tilesets;

    /// <summary>
    /// Gets or sets the optional camera for viewport culling optimization.
    /// </summary>
    public Camera2D Camera { get; set; }

    /// <summary>
    /// Gets or sets whether viewport culling is enabled.
    /// </summary>
    public System.Boolean UseViewportCulling { get; set; } = true;

    /// <summary>
    /// Gets or sets whether layers should be automatically sorted by render order.
    /// </summary>
    public System.Boolean AutoSortLayers { get; set; } = true;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMap"/> class.
    /// </summary>
    /// <param name="width">The width of the map in tiles.</param>
    /// <param name="height">The height of the map in tiles.</param>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when any parameter is less than or equal to zero.
    /// </exception>
    public TileMap(System.Int16 width, System.Int16 height, System.Int16 tileWidth, System.Int16 tileHeight)
    {
        if (width <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        if (tileWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(tileWidth), "Tile width must be greater than zero.");
        }

        if (tileHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(tileHeight), "Tile height must be greater than zero.");
        }

        Width = width;
        Height = height;
        TileWidth = tileWidth;
        TileHeight = tileHeight;

        _layers = [];
        _tilesets = [];
        _layerCache = new System.Collections.Generic.Dictionary<System.String, TileLayer>(
            System.StringComparer.Ordinal);
    }

    #endregion Constructor

    #region Layer Management

    /// <summary>
    /// Adds a layer to the map's rendering stack.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when layer is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when a layer with the same name already exists.</exception>
    public void AddLayer(TileLayer layer)
    {
        System.ArgumentNullException.ThrowIfNull(layer);

        if (_layerCache.ContainsKey(layer.Name))
        {
            throw new System.ArgumentException($"A layer with name '{layer.Name}' already exists.", nameof(layer));
        }

        _layers.Add(layer);
        _layerCache[layer.Name] = layer;
        _layersDirty = true;
    }

    /// <summary>
    /// Removes a layer from the map by name.
    /// </summary>
    /// <param name="layerName">The name of the layer to remove.</param>
    /// <returns><c>true</c> if the layer was removed; otherwise, <c>false</c>.</returns>
    public System.Boolean RemoveLayer(System.String layerName)
    {
        if (_layerCache.TryGetValue(layerName, out TileLayer layer))
        {
            _layers.Remove(layer);
            _layerCache.Remove(layerName);
            layer.Dispose();
            _layersDirty = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Retrieves a layer by its name (cached lookup).
    /// </summary>
    /// <param name="name">The name of the layer to find.</param>
    /// <returns>The layer, or <c>null</c> if not found.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public TileLayer GetLayer(System.String name)
    {
        _layerCache.TryGetValue(name, out TileLayer layer);
        return layer;
    }

    /// <summary>
    /// Retrieves all layers with a specific custom property value.
    /// </summary>
    /// <param name="propertyName">The property name to search for.</param>
    /// <param name="propertyValue">The property value to match.</param>
    /// <returns>A list of matching layers.</returns>
    public System.Collections.Generic.List<TileLayer> GetLayersWithProperty(
        System.String propertyName,
        System.String propertyValue)
    {
        System.Collections.Generic.List<TileLayer> result = [];

        foreach (TileLayer layer in _layers)
        {
            if (layer.Properties.TryGetValue(propertyName, out System.String value) &&
                value == propertyValue)
            {
                result.Add(layer);
            }
        }

        return result;
    }

    /// <summary>
    /// Sorts layers by their render order (based on LayerType).
    /// </summary>
    public void SortLayersByRenderOrder()
    {
        _layers.Sort((a, b) => a.RenderOrder.CompareTo(b.RenderOrder));
        _layersDirty = false;
    }

    #endregion Layer Management

    #region Tileset Management

    /// <summary>
    /// Adds a tileset to the map's tileset collection.
    /// </summary>
    /// <param name="tileset">The tileset to add.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when tileset is null.</exception>
    public void AddTileset(Tileset tileset)
    {
        System.ArgumentNullException.ThrowIfNull(tileset);

        _tilesets.Add(tileset);

        // Sort by FirstGid for efficient GID lookup
        _tilesets.Sort((a, b) => a.FirstGid.CompareTo(b.FirstGid));
    }

    /// <summary>
    /// Finds the tileset that contains the specified global tile ID (GID).
    /// </summary>
    /// <param name="gid">The global tile identifier.</param>
    /// <returns>The tileset containing the GID, or <c>null</c> if not found.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Tileset GetTilesetForGid(System.Int32 gid)
    {
        // Binary search would be better, but for small tileset counts, linear is fine
        for (System.Int32 i = _tilesets.Count - 1; i >= 0; i--)
        {
            if (_tilesets[i].ContainsGid(gid))
            {
                return _tilesets[i];
            }
        }

        return null;
    }

    #endregion Tileset Management

    #region Coordinate Conversion

    /// <summary>
    /// Converts a world position in pixels to tile coordinates.
    /// </summary>
    /// <param name="worldPos">The position in world space.</param>
    /// <returns>The tile coordinates.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Vector2i WorldToTile(Vector2f worldPos)
    {
        return new Vector2i(
            (System.Int32)(worldPos.X / TileWidth),
            (System.Int32)(worldPos.Y / TileHeight));
    }

    /// <summary>
    /// Converts tile coordinates to the world position of the tile's top-left corner.
    /// </summary>
    /// <param name="tilePos">The tile coordinates.</param>
    /// <returns>The world position in pixels.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Vector2f TileToWorld(Vector2i tilePos)
    {
        return new Vector2f(
            tilePos.X * TileWidth,
            tilePos.Y * TileHeight);
    }

    /// <summary>
    /// Converts tile coordinates to the world position of the tile's center point.
    /// </summary>
    /// <param name="tilePos">The tile coordinates.</param>
    /// <returns>The center position in pixels.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Vector2f TileToWorldCenter(Vector2i tilePos)
    {
        return new Vector2f(
            (tilePos.X * TileWidth) + (TileWidth * 0.5f),
            (tilePos.Y * TileHeight) + (TileHeight * 0.5f));
    }

    /// <summary>
    /// Determines whether the specified tile coordinates are within valid map bounds.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns><c>true</c> if within bounds; otherwise, <c>false</c>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsValidTileCoord(System.Int32 x, System.Int32 y)
    {
        return (System.UInt32)x < (System.UInt32)Width &&
               (System.UInt32)y < (System.UInt32)Height;
    }

    #endregion Coordinate Conversion

    #region Tile Queries

    /// <summary>
    /// Retrieves a tile from a specific layer at the given tile coordinates.
    /// </summary>
    /// <param name="layerName">The layer name.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>A readonly reference to the tile.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public ref readonly Tile GetTileAt(System.String layerName, System.Int32 x, System.Int32 y)
    {
        TileLayer layer = GetLayer(layerName);
        return ref layer is null ? ref System.Runtime.CompilerServices.Unsafe.NullRef<Tile>() : ref layer.GetTileRef(x, y);
    }

    /// <summary>
    /// Retrieves a tile from a specific layer at the given world position.
    /// </summary>
    /// <param name="layerName">The layer name.</param>
    /// <param name="worldPos">The world position.</param>
    /// <returns>A readonly reference to the tile.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public ref readonly Tile GetTileAtWorldPos(System.String layerName, Vector2f worldPos)
    {
        Vector2i tileCoord = WorldToTile(worldPos);
        return ref GetTileAt(layerName, tileCoord.X, tileCoord.Y);
    }

    /// <summary>
    /// Checks if a tile at the specified world position is collidable.
    /// </summary>
    /// <param name="layerName">The layer name.</param>
    /// <param name="worldPos">The world position.</param>
    /// <returns><c>true</c> if collidable; otherwise, <c>false</c>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsTileCollidable(System.String layerName, Vector2f worldPos)
    {
        ref readonly Tile tile = ref GetTileAtWorldPos(layerName, worldPos);
        return !System.Runtime.CompilerServices.Unsafe.IsNullRef(ref System.Runtime.CompilerServices.Unsafe.AsRef(in tile)) &&
               !tile.IsEmpty() &&
               tile.IsCollidable;
    }

    #endregion Tile Queries

    #region Building

    /// <summary>
    /// Builds vertex arrays for all layers to prepare them for rendering.
    /// </summary>
    public void BuildAllLayers()
    {
        foreach (TileLayer layer in _layers)
        {
            layer.BuildVertexArray(TileWidth, TileHeight);
        }

        if (AutoSortLayers)
        {
            SortLayersByRenderOrder();
        }
    }

    #endregion Building

    #region Update and Render

    /// <summary>
    /// Updates the tile map state for the current frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    public override void Update(System.Single deltaTime)
    {
        if (!IsEnabled)
        {
            return;
        }

        // Future: Update animated tiles here
        // Tương lai: Cập nhật animated tiles ở đây

        base.Update(deltaTime);
    }

    /// <summary>
    /// Renders the tile map to the specified render target.
    /// </summary>
    /// <param name="target">The render target.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override void Draw(RenderTarget target)
    {
        if (!IsVisible)
        {
            return;
        }

        // Sort layers if needed
        if (_layersDirty && AutoSortLayers)
        {
            SortLayersByRenderOrder();
        }

        // Calculate visible tile bounds if viewport culling is enabled
        FloatRect viewportBounds = default;
        System.Boolean usesCulling = UseViewportCulling && Camera is not null;

        if (usesCulling)
        {
            View cameraView = Camera.GetView();
            Vector2f center = cameraView.Center;
            Vector2f size = cameraView.Size;

            viewportBounds = new FloatRect(
                center.X - (size.X * 0.5f),
                center.Y - (size.Y * 0.5f),
                size.X,
                size.Y);
        }

        foreach (TileLayer layer in _layers)
        {
            if (!layer.Visible)
            {
                continue;
            }

            // TODO: Implement viewport culling per-layer
            // For now, render entire layer
            layer.Draw(target);
        }
    }

    /// <inheritdoc/>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("TileMap uses custom Draw() method.");

    #endregion Update and Render

    #region Lifecycle

    /// <summary>
    /// Performs cleanup before the tile map is destroyed.
    /// </summary>
    public override void OnBeforeDestroy()
    {
        Dispose();
        base.OnBeforeDestroy();
    }

    #endregion Lifecycle

    #region IDisposable

    /// <summary>
    /// Releases all resources used by the tile map.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (TileLayer layer in _layers)
        {
            layer.Dispose();
        }

        _layers.Clear();
        _layerCache.Clear();
        _tilesets.Clear();

        _disposed = true;
        System.GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}