// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tiles;

/// <summary>
/// Represents a complete tile-based map that manages layers, tilesets, coordinate transformations, and rendering.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TileMap"/> class is the primary container for tile-based level data, supporting multiple
/// layers and tilesets. It provides coordinate conversion utilities, collision queries, and efficient rendering
/// through layer-based vertex array batching.
/// </para>
/// <para>
/// This class inherits from <see cref="RenderObject"/> and integrates into the game's rendering pipeline.
/// It supports optional viewport culling via <see cref="Camera2D"/> for improved performance with large maps.
/// </para>
/// </remarks>
public sealed class TileMap : RenderObject
{
    #region Properties

    /// <summary>
    /// Gets the width of the map in tiles.
    /// </summary>
    /// <value>
    /// The number of tiles horizontally across the map. This value is immutable after construction.
    /// </value>
    public System.Int16 Width { get; }

    /// <summary>
    /// Gets the height of the map in tiles.
    /// </summary>
    /// <value>
    /// The number of tiles vertically down the map. This value is immutable after construction.
    /// </value>
    public System.Int16 Height { get; }

    /// <summary>
    /// Gets the width of each tile in pixels.
    /// </summary>
    /// <value>
    /// The horizontal size of a single tile in pixels. This value is immutable after construction.
    /// Typical values are 16, 32, or 64 pixels.
    /// </value>
    public System.Int16 TileWidth { get; }

    /// <summary>
    /// Gets the height of each tile in pixels.
    /// </summary>
    /// <value>
    /// The vertical size of a single tile in pixels. This value is immutable after construction.
    /// Typical values are 16, 32, or 64 pixels.
    /// </value>
    public System.Int16 TileHeight { get; }

    /// <summary>
    /// Gets the total width of the map in pixels.
    /// </summary>
    /// <value>
    /// Calculated as <see cref="Width"/> × <see cref="TileWidth"/>.
    /// Represents the full horizontal extent of the map in world coordinates.
    /// </value>
    public System.Int32 PixelWidth => Width * TileWidth;

    /// <summary>
    /// Gets the total height of the map in pixels.
    /// </summary>
    /// <value>
    /// Calculated as <see cref="Height"/> × <see cref="TileHeight"/>.
    /// Represents the full vertical extent of the map in world coordinates.
    /// </value>
    public System.Int32 PixelHeight => Height * TileHeight;

    /// <summary>
    /// Gets the list of all layers in the map.
    /// </summary>
    /// <value>
    /// An ordered collection of <see cref="TileLayer"/> objects. Layers are rendered in the order they appear
    /// in this list (earlier layers are drawn first, later layers are drawn on top).
    /// This collection is never <c>null</c> but may be empty.
    /// </value>
    public System.Collections.Generic.List<TileLayer> Layers { get; }

    /// <summary>
    /// Gets the list of all tilesets used by this map.
    /// </summary>
    /// <value>
    /// A collection of <see cref="Tileset"/> objects containing texture atlases and tile metadata.
    /// This collection is never <c>null</c> but may be empty.
    /// Tilesets should be ordered by their FirstGid for correct tile resolution.
    /// </value>
    public System.Collections.Generic.List<Tileset> Tilesets { get; }

    /// <summary>
    /// Gets or sets the optional camera for viewport culling optimization.
    /// </summary>
    /// <value>
    /// A <see cref="Camera2D"/> instance used to determine the visible area of the map.
    /// When set and <see cref="UseViewportCulling"/> is <c>true</c>, only tiles within the camera's
    /// view are rendered. May be <c>null</c> if viewport culling is not used.
    /// </value>
    public Camera2D Camera { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether viewport culling is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> to enable viewport culling (only render tiles visible in the <see cref="Camera"/>);
    /// <c>false</c> to render the entire map. Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// Viewport culling significantly improves performance for large maps by skipping tiles outside
    /// the camera's view. This feature requires a valid <see cref="Camera"/> to be set.
    /// </remarks>
    public System.Boolean UseViewportCulling { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMap"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the map in tiles. Must be greater than zero.</param>
    /// <param name="height">The height of the map in tiles. Must be greater than zero.</param>
    /// <param name="tileWidth">The width of each tile in pixels. Must be greater than zero.</param>
    /// <param name="tileHeight">The height of each tile in pixels. Must be greater than zero.</param>
    /// <remarks>
    /// Creates an empty tile map with no layers or tilesets. Viewport culling is enabled by default.
    /// Use <see cref="AddLayer"/> and <see cref="AddTileset"/> to populate the map, then call
    /// <see cref="BuildAllLayers"/> to prepare for rendering.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when any of the parameters is less than or equal to zero.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Explicit initialization pattern preferred for clarity and future extensibility")]
    public TileMap(System.Int16 width, System.Int16 height, System.Int16 tileWidth, System.Int16 tileHeight)
    {
        Width = width;
        Height = height;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        UseViewportCulling = true;

        Layers = [];
        Tilesets = [];
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Adds a layer to the map's rendering stack.
    /// </summary>
    /// <param name="layer">The <see cref="TileLayer"/> to add. If <c>null</c>, this method does nothing.</param>
    /// <remarks>
    /// Layers are rendered in the order they are added. After adding layers, call <see cref="BuildAllLayers"/>
    /// to prepare them for rendering.
    /// </remarks>
    public void AddLayer(TileLayer layer)
    {
        if (layer != null)
        {
            Layers.Add(layer);
        }
    }

    /// <summary>
    /// Adds a tileset to the map's tileset collection.
    /// </summary>
    /// <param name="tileset">The <see cref="Tileset"/> to add. If <c>null</c>, this method does nothing.</param>
    /// <remarks>
    /// Tilesets should be added in order of their FirstGid (lowest to highest) to ensure correct
    /// tile resolution via <see cref="GetTilesetForGid"/>.
    /// </remarks>
    public void AddTileset(Tileset tileset)
    {
        if (tileset != null)
        {
            Tilesets.Add(tileset);
        }
    }

    /// <summary>
    /// Retrieves a layer by its name.
    /// </summary>
    /// <param name="name">The name of the layer to find. Case-sensitive.</param>
    /// <returns>
    /// The first <see cref="TileLayer"/> with a matching name, or <c>null</c> if no layer is found.
    /// </returns>
    /// <remarks>
    /// If multiple layers have the same name, only the first match is returned.
    /// Layer names are typically unique within a tile map.
    /// </remarks>
    public TileLayer GetLayer(System.String name) => Layers.Find(l => l.Name == name);

    /// <summary>
    /// Finds the tileset that contains the specified global tile ID (GID).
    /// </summary>
    /// <param name="gid">The global tile identifier to search for.</param>
    /// <returns>
    /// The <see cref="Tileset"/> that contains the specified GID, or <c>null</c> if no tileset contains it.
    /// </returns>
    /// <remarks>
    /// This method searches tilesets in reverse order (highest FirstGid to lowest) to correctly handle
    /// overlapping GID ranges. This is the standard behavior for Tiled map format.
    /// </remarks>
    public Tileset GetTilesetForGid(System.Int32 gid)
    {
        // Search from highest firstgid to lowest to find the correct tileset
        for (System.Int32 i = Tilesets.Count - 1; i >= 0; i--)
        {
            if (Tilesets[i].ContainsGid(gid))
            {
                return Tilesets[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Builds vertex arrays for all layers to prepare them for efficient rendering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called after all tiles have been set in all layers, typically after
    /// loading a map from file or procedurally generating map data.
    /// </para>
    /// <para>
    /// Building vertex arrays is a relatively expensive operation and should not be called every frame.
    /// Only rebuild when tile data changes.
    /// </para>
    /// </remarks>
    public void BuildAllLayers()
    {
        foreach (TileLayer layer in Layers)
        {
            layer.BuildVertexArray(TileWidth, TileHeight);
        }
    }

    /// <summary>
    /// Converts a world position in pixels to tile coordinates.
    /// </summary>
    /// <param name="worldPos">The position in world space (pixels).</param>
    /// <returns>
    /// A <see cref="Vector2i"/> representing the tile coordinates. The result may be outside
    /// the valid map bounds if <paramref name="worldPos"/> is outside the map area.
    /// </returns>
    /// <remarks>
    /// This method performs integer division and truncates towards zero. Use <see cref="IsValidTileCoord"/>
    /// to verify the result is within map bounds. This method is marked with
    /// <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/> for optimal performance.
    /// </remarks>
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
    /// <param name="tilePos">The tile coordinates to convert.</param>
    /// <returns>
    /// A <see cref="Vector2f"/> representing the pixel position of the top-left corner of the tile.
    /// </returns>
    /// <remarks>
    /// This is the inverse operation of <see cref="WorldToTile"/>. The returned position represents
    /// the top-left corner of the tile in world space. This method is marked with
    /// <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/> for optimal performance.
    /// </remarks>
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
    /// <param name="tilePos">The tile coordinates to convert.</param>
    /// <returns>
    /// A <see cref="Vector2f"/> representing the pixel position of the center of the tile.
    /// </returns>
    /// <remarks>
    /// This is useful for positioning entities at the center of a tile rather than at the top-left corner.
    /// The center is calculated as the top-left corner plus half the tile dimensions. This method is marked with
    /// <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/> for optimal performance.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Vector2f TileToWorldCenter(Vector2i tilePos)
    {
        return new Vector2f(
            (tilePos.X * TileWidth) + (TileWidth / 2f),
            (tilePos.Y * TileHeight) + (TileHeight / 2f));
    }

    /// <summary>
    /// Determines whether the specified tile coordinates are within the valid map bounds.
    /// </summary>
    /// <param name="x">The X coordinate (column) in tile space.</param>
    /// <param name="y">The Y coordinate (row) in tile space.</param>
    /// <returns>
    /// <c>true</c> if the coordinates are within bounds (0 ≤ x &lt; <see cref="Width"/> and 0 ≤ y &lt; <see cref="Height"/>);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is commonly used before accessing tile data to prevent out-of-bounds errors.
    /// It is marked with <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance in tight loops.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsValidTileCoord(System.Int32 x, System.Int32 y) => x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>
    /// Retrieves a tile from a specific layer at the given tile coordinates.
    /// </summary>
    /// <param name="layerName">The name of the layer to query. Case-sensitive.</param>
    /// <param name="x">The X coordinate (column) in tile space.</param>
    /// <param name="y">The Y coordinate (row) in tile space.</param>
    /// <returns>
    /// The <see cref="Tile"/> at the specified position, or <c>null</c> if the layer does not exist,
    /// the coordinates are out of bounds, or no tile exists at that position.
    /// </returns>
    public Tile GetTileAt(System.String layerName, System.Int32 x, System.Int32 y)
    {
        TileLayer layer = GetLayer(layerName);
        return layer?.GetTile(x, y);
    }

    /// <summary>
    /// Retrieves a tile from a specific layer at the given world position.
    /// </summary>
    /// <param name="layerName">The name of the layer to query. Case-sensitive.</param>
    /// <param name="worldPos">The position in world space (pixels).</param>
    /// <returns>
    /// The <see cref="Tile"/> at the specified world position, or <c>null</c> if the layer does not exist,
    /// the position is outside the map, or no tile exists at that position.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that internally calls <see cref="WorldToTile"/> to convert coordinates.
    /// </remarks>
    public Tile GetTileAtWorldPos(System.String layerName, Vector2f worldPos)
    {
        TileLayer layer = GetLayer(layerName);
        if (layer == null)
        {
            return null;
        }

        Vector2i tileCoord = WorldToTile(worldPos);
        return layer.GetTile(tileCoord.X, tileCoord.Y);
    }

    /// <summary>
    /// Checks if a tile at the specified world position is collidable.
    /// </summary>
    /// <param name="layerName">The name of the collision layer to check. Case-sensitive.</param>
    /// <param name="worldPos">The position in world space (pixels) to test.</param>
    /// <returns>
    /// <c>true</c> if a collidable tile exists at the specified position; otherwise, <c>false</c>.
    /// Returns <c>false</c> if the layer does not exist or the position is outside the map.
    /// </returns>
    /// <remarks>
    /// This is a convenience method commonly used for collision detection queries.
    /// </remarks>
    public System.Boolean IsTileCollidable(System.String layerName, Vector2f worldPos)
    {
        Tile tile = GetTileAtWorldPos(layerName, worldPos);
        return tile?.IsCollidable == true;
    }

    /// <summary>
    /// Retrieves all layers that have a specific custom property with the specified value.
    /// </summary>
    /// <param name="propertyName">The name of the property to search for. Case-sensitive.</param>
    /// <param name="propertyValue">The value of the property to match. Case-sensitive.</param>
    /// <returns>
    /// A list of <see cref="TileLayer"/> objects that have the specified property-value pair.
    /// Returns an empty list if no matching layers are found. The returned list is never <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for finding special layers by their metadata, such as finding all
    /// "collision" layers or "background" layers based on Tiled editor properties.
    /// </remarks>
    public System.Collections.Generic.List<TileLayer> GetLayersWithProperty(System.String propertyName, System.String propertyValue)
    {
        System.Collections.Generic.List<TileLayer> result = [];

        foreach (TileLayer layer in Layers)
        {
            if (layer.Properties.TryGetValue(propertyName, out System.String value))
            {
                if (value == propertyValue)
                {
                    result.Add(layer);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Updates the tile map state for the current frame.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update in seconds.</param>
    /// <remarks>
    /// Currently, this method is a placeholder for future animated tile support.
    /// The map is only updated if <see cref="RenderObject.IsEnabled"/> is <c>true</c>.
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!base.IsEnabled)
        {
            return;
        }

        // Future: Update animated tiles here
        // Tương lai: Cập nhật các tiles có animation ở đây

        base.Update(deltaTime);
    }

    /// <summary>
    /// Renders the tile map to the specified render target.
    /// </summary>
    /// <param name="target">The SFML render target to draw to (e.g., RenderWindow or RenderTexture).</param>
    /// <remarks>
    /// <para>
    /// This method iterates through all visible layers and renders them in order using their
    /// associated tilesets. The map is only drawn if <see cref="RenderObject.IsVisible"/> is <c>true</c>.
    /// </para>
    /// <para>
    /// Current limitations:
    /// </para>
    /// <list type="bullet">
    /// <item>Only the first tileset is used for rendering all layers (multi-tileset support pending).</item>
    /// <item>Viewport culling is not yet implemented (all tiles are drawn regardless of camera view).</item>
    /// </list>
    /// </remarks>
    public override void Draw(RenderTarget target)
    {
        if (!base.IsVisible)
        {
            return;
        }

        // Draw each layer
        foreach (TileLayer layer in Layers)
        {
            if (!layer.Visible)
            {
                continue;
            }

            // Find the tileset for this layer (use first tileset for now)
            // TODO: Support multiple tilesets per layer
            Tileset tileset = Tilesets.Count > 0 ? Tilesets[0] : null;
            if (tileset?.Texture == null)
            {
                continue;
            }

            // TODO: Add viewport culling here if UseViewportCulling is true
            // For now, just draw the entire layer
            layer.Draw(target);
        }
    }

    /// <summary>
    /// Gets the drawable object for this tile map.
    /// </summary>
    /// <returns>This method is not supported for <see cref="TileMap"/>.</returns>
    /// <exception cref="System.NotSupportedException">
    /// Always thrown. <see cref="TileMap"/> uses a custom <see cref="Draw"/> implementation
    /// rather than providing a single <see cref="Drawable"/> object.
    /// </exception>
    protected override Drawable GetDrawable() => throw new System.NotSupportedException("TileMap uses custom Draw() method.");

    /// <summary>
    /// Performs cleanup operations before the tile map is destroyed.
    /// </summary>
    /// <remarks>
    /// This method disposes all layers and clears the layer and tileset collections.
    /// It is automatically called by the engine's lifecycle management system.
    /// After this method completes, the tile map should not be used for rendering.
    /// </remarks>
    public override void OnBeforeDestroy()
    {
        foreach (TileLayer layer in Layers)
        {
            layer.Dispose();
        }

        Layers.Clear();
        Tilesets.Clear();

        base.OnBeforeDestroy();
    }

    #endregion Methods
}