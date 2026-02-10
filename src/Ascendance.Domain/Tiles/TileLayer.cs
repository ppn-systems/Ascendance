// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Domain.Enums;
using Ascendance.Domain.Extensions;
using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Domain.Tiles;

/// <summary>
/// Represents a single renderable layer in a tile map with efficient vertex-based rendering.
/// </summary>
public sealed class TileLayer : RenderObject, System.IDisposable
{
    #region Fields

    private readonly Tile[] _tiles;

    private VertexArray _vertexArray;
    private System.Boolean _disposed;

    #endregion Fields

    #region Properties

    /// <summary>
    /// The texture atlas used for this tile layer.
    /// </summary>
    public Texture Texture { get; set; }

    /// <summary>
    /// Gets or sets the name of the layer.
    /// </summary>
    public System.String Name { get; set; }

    /// <summary>
    /// Gets or sets the layer type classification for rendering order and purpose.
    /// </summary>
    public TileLayerType LayerType { get; set; } = TileLayerType.Ground;

    /// <summary>
    /// Gets the width of the layer in tiles.
    /// </summary>
    public System.Int16 Width { get; }

    /// <summary>
    /// Gets the height of the layer in tiles.
    /// </summary>
    public System.Int16 Height { get; }

    /// <summary>
    /// Gets or sets whether this layer is visible.
    /// </summary>
    public System.Boolean Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the opacity of the layer (0.0 to 1.0).
    /// </summary>
    public System.Single Opacity { get; set; } = 1.0f;

    /// <summary>
    /// Gets the custom properties imported from Tiled editor.
    /// </summary>
    public System.Collections.Generic.Dictionary<System.String, System.String> Properties { get; }

    /// <summary>
    /// Gets the render order based on layer type.
    /// </summary>
    public System.Int16 RenderOrder => LayerType.GetRenderOrder();

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TileLayer"/> class.
    /// </summary>
    /// <param name="width">The width of the layer in tiles.</param>
    /// <param name="height">The height of the layer in tiles.</param>
    public TileLayer(System.Int16 width, System.Int16 height)
    {
        _tiles = new Tile[width * height];

        Width = width;
        Height = height;
        Properties = [];
        Visible = LayerType.IsVisibleByDefault();
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Gets a reference to the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>A readonly reference to the tile.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public ref readonly Tile GetTileRef(System.Int32 x, System.Int32 y)
        => ref x < 0 || x >= Width || y < 0 || y >= Height ? ref System.Runtime.CompilerServices.Unsafe.NullRef<Tile>() : ref _tiles[(y * Width) + x];

    /// <summary>
    /// Sets the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="tile">The tile to set.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetTile(System.Int32 x, System.Int32 y, Tile tile)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _tiles[(y * Width) + x] = tile;
        }
    }

    /// <summary>
    /// Gets a span of all tiles in this layer.
    /// </summary>
    /// <returns>A span containing all tiles.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Span<Tile> GetTilesSpan() => System.MemoryExtensions.AsSpan(_tiles);

    /// <summary>
    /// Builds or rebuilds the vertex array for efficient batch rendering.
    /// </summary>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void BuildVertexArray(System.Int32 tileWidth, System.Int32 tileHeight)
    {
        _vertexArray?.Dispose();
        _vertexArray = new VertexArray(PrimitiveType.Triangles);

        System.Byte alpha = (System.Byte)(Opacity * 255);
        Color color = new(255, 255, 255, alpha);

        System.Span<Tile> tiles = System.MemoryExtensions.AsSpan(_tiles);
        System.Int32 index = 0;

        for (System.Int32 y = 0; y < Height; y++)
        {
            for (System.Int32 x = 0; x < Width; x++, index++)
            {
                ref readonly Tile tile = ref tiles[index];
                if (tile.IsEmpty())
                {
                    continue;
                }

                System.Single worldX = x * tileWidth;
                System.Single worldY = y * tileHeight;

                IntRect texRect = tile.TextureRect;

                // Triangle 1
                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX, worldY),
                    color,
                    new Vector2f(texRect.Left, texRect.Top)));

                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top)));

                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left, texRect.Top + texRect.Height)));

                // Triangle 2
                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left, texRect.Top + texRect.Height)));

                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top)));

                _vertexArray.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top + texRect.Height)));
            }
        }
    }

    /// <summary>
    /// Renders the tile layer to the specified render target.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!Visible || _vertexArray is null)
        {
            return;
        }

        RenderStates states = new(texture: Texture);
        target.Draw(_vertexArray, states);
    }

    /// <inheritdoc/>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() method instead.");

    #endregion Methods

    #region IDisposable

    /// <summary>
    /// Releases resources used by the tile layer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _vertexArray?.Dispose();
        _vertexArray = null;
        _disposed = true;

        System.GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}