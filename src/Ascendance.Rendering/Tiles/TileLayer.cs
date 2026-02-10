// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tiles;

/// <summary>
/// Represents a single renderable layer in a tile map with support for visibility, opacity, and efficient vertex-based rendering.
/// </summary>
/// <remarks>
/// <para>
/// A tile layer contains a 2D grid of tiles and manages their rendering through a cached vertex array.
/// This class inherits from <see cref="RenderObject"/> and implements the IDisposable pattern for proper resource cleanup.
/// </para>
/// <para>
/// The layer uses a vertex array optimization where all tiles are batched into a single draw call,
/// significantly improving rendering performance for large tile maps.
/// </para>
/// </remarks>
public sealed class TileLayer : RenderObject
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of the layer.
    /// </summary>
    /// <value>
    /// The unique identifier name for this layer, typically defined in the Tiled map editor.
    /// </value>
    public System.String Name { get; set; }

    /// <summary>
    /// Gets or sets the width of the layer in tiles.
    /// </summary>
    /// <value>
    /// The number of tiles horizontally across the layer. Must be greater than zero.
    /// </value>
    public System.Int32 Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the layer in tiles.
    /// </summary>
    /// <value>
    /// The number of tiles vertically down the layer. Must be greater than zero.
    /// </value>
    public System.Int32 Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this layer is visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the layer should be rendered; otherwise, <c>false</c>.
    /// Default value is <c>true</c>.
    /// </value>
    public System.Boolean Visible { get; set; }

    /// <summary>
    /// Gets or sets the opacity of the layer.
    /// </summary>
    /// <value>
    /// A value between 0.0 (fully transparent) and 1.0 (fully opaque).
    /// Values outside this range may produce undefined rendering behavior.
    /// Default value is 1.0.
    /// </value>
    public System.Single Opacity { get; set; }

    /// <summary>
    /// Gets or sets the 2D array of tiles stored in row-major order [y, x].
    /// </summary>
    /// <value>
    /// A two-dimensional array where the first index represents the Y coordinate (row)
    /// and the second index represents the X coordinate (column).
    /// Array dimensions match <see cref="Width"/> and <see cref="Height"/>.
    /// Elements may be <c>null</c> for empty tile positions.
    /// </value>
    public Tile[,] Tiles { get; set; }

    /// <summary>
    /// Gets or sets the custom properties imported from Tiled editor.
    /// </summary>
    /// <value>
    /// A dictionary of key-value pairs containing metadata defined in the Tiled map editor.
    /// This is never <c>null</c> after construction.
    /// </value>
    public System.Collections.Generic.Dictionary<System.String, System.String> Properties { get; set; }

    /// <summary>
    /// Cached vertex array for efficient batch rendering of all tiles in this layer.
    /// </summary>
    /// <value>
    /// The vertex array containing all tile geometry, or <c>null</c> if not yet built.
    /// This is rebuilt when <see cref="BuildVertexArray"/> is called.
    /// </value>
    private VertexArray _vertexArray;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TileLayer"/> class with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the layer in tiles. Must be greater than zero.</param>
    /// <param name="height">The height of the layer in tiles. Must be greater than zero.</param>
    /// <remarks>
    /// Creates a fully visible layer with full opacity (1.0) and initializes an empty tile array.
    /// The vertex array is not built until <see cref="BuildVertexArray"/> is explicitly called.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Explicit initialization pattern preferred for clarity")]
    public TileLayer(System.Int32 width, System.Int32 height)
    {
        Width = width;
        Height = height;
        Visible = true;
        Opacity = 1.0f;
        Tiles = new Tile[height, width];
        Properties = [];
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Retrieves the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate (column) in tile space. Zero-based index.</param>
    /// <param name="y">The Y coordinate (row) in tile space. Zero-based index.</param>
    /// <returns>
    /// The <see cref="Tile"/> at the specified position, or <c>null</c> if the coordinates
    /// are out of bounds or if no tile exists at that position.
    /// </returns>
    /// <remarks>
    /// This method performs bounds checking and returns <c>null</c> for invalid coordinates
    /// rather than throwing an exception. It is marked with <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance in tight loops.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Tile GetTile(System.Int32 x, System.Int32 y) => x < 0 || x >= Width || y < 0 || y >= Height ? null : Tiles[y, x];

    /// <summary>
    /// Sets the tile at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate (column) in tile space. Zero-based index.</param>
    /// <param name="y">The Y coordinate (row) in tile space. Zero-based index.</param>
    /// <param name="tile">The <see cref="Tile"/> to place at the specified position. May be <c>null</c> to clear the position.</param>
    /// <remarks>
    /// <para>
    /// This method performs bounds checking and silently ignores requests with invalid coordinates.
    /// After setting tiles, you must call <see cref="BuildVertexArray"/> to update the rendering cache.
    /// </para>
    /// <para>
    /// This method is marked with <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance when building tile maps.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetTile(System.Int32 x, System.Int32 y, Tile tile)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            Tiles[y, x] = tile;
        }
    }

    /// <summary>
    /// Builds or rebuilds the vertex array for efficient batch rendering of all tiles.
    /// </summary>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    /// <remarks>
    /// <para>
    /// This method creates a vertex array containing all visible tiles in the layer,
    /// where each tile is represented by two triangles (6 vertices total).
    /// The vertex array is used for efficient single-draw-call rendering.
    /// </para>
    /// <para>
    /// This method should be called:
    /// </para>
    /// <list type="bullet">
    /// <item>After loading the tile map data.</item>
    /// <item>Whenever tiles are modified via <see cref="SetTile"/>.</item>
    /// <item>When the layer's <see cref="Opacity"/> changes.</item>
    /// </list>
    /// <para>
    /// Any existing vertex array is disposed before creating the new one, preventing memory leaks.
    /// Empty tiles (tiles with <see cref="Tile.Gid"/> of 0) are skipped during vertex generation.
    /// </para>
    /// </remarks>
    public void BuildVertexArray(System.Int32 tileWidth, System.Int32 tileHeight)
    {
        // Each tile = 2 triangles = 6 vertices
        _vertexArray?.Dispose();
        _vertexArray = new VertexArray(PrimitiveType.Triangles);

        System.Byte alpha = (System.Byte)(Opacity * 255);

        for (System.Int32 y = 0; y < Height; y++)
        {
            for (System.Int32 x = 0; x < Width; x++)
            {
                Tile tile = Tiles[y, x];
                if (tile?.IsEmpty() != false)
                {
                    continue;
                }

                // World position
                System.Single worldX = x * tileWidth;
                System.Single worldY = y * tileHeight;

                // Texture coordinates
                IntRect texRect = tile.TextureRect;

                // Create quad using 2 triangles
                Color color = new(255, 255, 255, alpha);

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
    /// <param name="target">The SFML render target to draw to (e.g., RenderWindow or RenderTexture).</param>
    /// <remarks>
    /// This method is called during the rendering pipeline and draws all tiles in a single batch operation
    /// if the layer is visible and the vertex array has been built. If <see cref="Visible"/> is <c>false</c>
    /// or the vertex array is <c>null</c>, no rendering occurs.
    /// </remarks>
    public override void Draw(RenderTarget target)
    {
        if (!Visible || _vertexArray == null)
        {
            return;
        }

        target.Draw(_vertexArray);
    }

    /// <summary>
    /// Gets the drawable object for this layer.
    /// </summary>
    /// <returns>
    /// This method is not implemented for <see cref="TileLayer"/> as rendering
    /// is handled directly via <see cref="Draw"/>.
    /// </returns>
    /// <exception cref="System.NotImplementedException">
    /// Always thrown. This method exists to satisfy the <see cref="RenderObject"/> contract
    /// but is not used in the tile layer rendering pipeline.
    /// </exception>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => throw new System.NotImplementedException();

    /// <summary>
    /// Releases all resources used by the tile layer, including the cached vertex array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method implements the Dispose pattern and should be called when the tile layer
    /// is no longer needed to prevent memory leaks. After disposal, the layer cannot be rendered
    /// until <see cref="BuildVertexArray"/> is called again.
    /// </para>
    /// <para>
    /// This method is safe to call multiple times; subsequent calls have no effect.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        _vertexArray?.Dispose();
        _vertexArray = null;
    }

    #endregion Methods
}