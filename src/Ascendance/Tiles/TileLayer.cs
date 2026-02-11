// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Shared.Enums;
using Ascendance.Shared.Extensions;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Ascendance.Tiles;

/// <summary>
/// Represents a single renderable layer in a tile map with efficient vertex-based rendering.
/// Supports multi-texture batching for layers that contain tiles from multiple tilesets.
/// </summary>
public sealed class TileLayer : RenderObject, System.IDisposable
{
    #region Fields

    private readonly Tile[] _tiles;

    // Support multiple batches (one VertexArray per texture atlas)
    private System.Boolean _disposed;
    private List<Batch> _batches;

    #endregion Fields

    #region Properties

    public Texture Texture { get; set; }

    public System.String Name { get; set; }

    public TileLayerType LayerType { get; set; } = TileLayerType.Ground;

    public System.Int16 Width { get; }

    public System.Int16 Height { get; }

    public System.Boolean Visible { get; set; } = true;

    public System.Single Opacity { get; set; } = 1.0f;

    public Dictionary<System.String, System.String> Properties { get; }

    public System.Int16 RenderOrder => LayerType.GetRenderOrder();

    #endregion Properties

    #region Inner types

    private sealed class Batch : System.IDisposable
    {
        public Texture Texture;
        public VertexArray Va;

        public void Dispose()
        {
            Va?.Dispose();
            Va = null;
            Texture = null;
        }

        void System.IDisposable.Dispose() => Dispose();
    }

    #endregion Inner types

    #region Constructor

    public TileLayer(System.Int16 width, System.Int16 height)
    {
        _tiles = new Tile[width * height];

        Width = width;
        Height = height;
        Properties = new Dictionary<System.String, System.String>(System.StringComparer.Ordinal);
        Visible = LayerType.IsVisibleByDefault();
        _batches = [];
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Gets a reference to the tile at the specified coordinates.
    /// </summary>
    public ref readonly Tile GetTileRef(System.Int32 x, System.Int32 y)
        => ref x < 0 || x >= Width || y < 0 || y >= Height ? ref System.Runtime.CompilerServices.Unsafe.NullRef<Tile>() : ref _tiles[(y * Width) + x];

    /// <summary>
    /// Sets the tile at the specified coordinates.
    /// </summary>
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
    public System.Span<Tile> GetTilesSpan() => System.MemoryExtensions.AsSpan(_tiles);

    /// <summary>
    /// Builds or rebuilds the vertex array(s) for efficient batch rendering.
    /// Groups vertices by texture atlas to support multi-tileset layers.
    /// </summary>
    public void BuildVertexArray(System.Int32 tileWidth, System.Int32 tileHeight)
    {
        // Dispose old batches
        if (_batches != null)
        {
            foreach (var b in _batches)
            {
                b.Dispose();
            }
            _batches.Clear();
        }
        else
        {
            _batches = [];
        }

        // Map texture -> vertex array batch
        Dictionary<Texture, VertexArray> dict = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

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

                Texture atlas = tile.Atlas ?? Texture;
                if (atlas is null)
                {
                    // If we don't have a texture for this tile, skip it.
                    continue;
                }

                if (!dict.TryGetValue(atlas, out VertexArray va))
                {
                    va = new VertexArray(PrimitiveType.Triangles);
                    dict[atlas] = va;
                }

                System.Single worldX = x * tileWidth;
                System.Single worldY = y * tileHeight;

                IntRect texRect = tile.TextureRect;

                // Triangle 1
                va.Append(new Vertex(
                    new Vector2f(worldX, worldY),
                    color,
                    new Vector2f(texRect.Left, texRect.Top)));

                va.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top)));

                va.Append(new Vertex(
                    new Vector2f(worldX, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left, texRect.Top + texRect.Height)));

                // Triangle 2
                va.Append(new Vertex(
                    new Vector2f(worldX, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left, texRect.Top + texRect.Height)));

                va.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top)));

                va.Append(new Vertex(
                    new Vector2f(worldX + tileWidth, worldY + tileHeight),
                    color,
                    new Vector2f(texRect.Left + texRect.Width, texRect.Top + texRect.Height)));
            }
        }

        // Convert dictionary to batches and store
        foreach (var kvp in dict)
        {
            _batches.Add(new Batch { Texture = kvp.Key, Va = kvp.Value });
        }
    }

    /// <summary>
    /// Renders the tile layer to the specified render target.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!Visible || _batches == null || _batches.Count == 0)
        {
            return;
        }

        foreach (Batch b in _batches)
        {
            if (b.Texture is null || b.Va is null)
            {
                continue;
            }

            RenderStates states = new(b.Texture);
            target.Draw(b.Va, states);
        }
    }

    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() method instead.");

    #endregion Methods

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_batches != null)
        {
            foreach (var b in _batches)
            {
                b.Dispose();
            }
            _batches.Clear();
            _batches = null;
        }

        _disposed = true;
        System.GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}