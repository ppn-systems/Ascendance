// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Entities;

/// <summary>
/// Efficiently draws multiple sprites in a single draw call with support for custom position, scale, rotation, color, animation frames and extra user data.
/// Supports singleton instance per T (using SingletonBase).
/// </summary>
/// <typeparam name="T">
/// Type of extra metadata stored per batch item, must be a reference type with parameterless constructor.
/// </typeparam>
public class SpriteBatch<T> : SingletonBase<SpriteBatch<T>>
    where T : class, new()
{
    #region Fields and Structs

    private Texture _texture;
    private readonly VertexArray _vertices;
    private readonly System.Collections.Generic.List<BatchItem> _items;

    /// <summary>
    /// Represents a single batched sprite with transform, appearance, and custom data.
    /// </summary>
    private struct BatchItem(Vector2f pos, IntRect src, Color color, Vector2f scale, System.Single rot, Vector2f origin, T extra)
    {
        public Vector2f Position = pos;
        public IntRect SourceRect = src;
        public Color Color = color;
        public Vector2f Scale = scale;
        public System.Single Rotation = rot;
        public Vector2f Origin = origin;
        public T Extra = extra; // Custom data
    }

    #endregion Fields and Structs

    #region Construction

    /// <summary>
    /// Initializes a new SpriteBatch for the given texture atlas.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public SpriteBatch()
    {
        _vertices = new VertexArray(PrimitiveType.Quads);
        _items = new System.Collections.Generic.List<BatchItem>(512);
    }

    #endregion Construction

    #region APIs

    /// <summary>
    /// Clears all batched sprites for a new frame.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _vertices.Clear();
    }

    /// <summary>
    /// Adds a sprite to the batch with transform, color, and custom user data.
    /// </summary>
    /// <param name="position">Position of the sprite on screen.</param>
    /// <param name="sourceRect">Rectangle on the texture (animation frame).</param>
    /// <param name="color">Pixel color (tint and alpha). Default white.</param>
    /// <param name="scale">Scaling factor. Default (1,1).</param>
    /// <param name="rotation">Rotation angle in degrees. Default 0.</param>
    /// <param name="origin">Origin within the frame for transform center. Default (0,0).</param>
    /// <param name="extra">Custom per-sprite data (type T). If null, new T() is used.</param>
    public void Add(
        Vector2f position,
        IntRect sourceRect,
        Color? color = null,
        Vector2f? scale = null,
        System.Single rotation = 0f,
        Vector2f? origin = null,
        T extra = null)
    {
        _items.Add(new BatchItem(
            position,
            sourceRect,
            color ?? Color.White,
            scale ?? new Vector2f(1f, 1f),
            rotation,
            origin ?? new Vector2f(0f, 0f),
            extra ?? new T()
        ));
    }

    /// <summary>
    /// Sets the texture atlas used for all batched sprites.
    /// </summary>
    public void SetTexture(Texture texture) => _texture = texture;

    /// <summary>
    /// Draws all batched sprites to the target in a single call.
    /// </summary>
    /// <param name="target">The render target (usually your window).</param>
    public void Draw(RenderTarget target)
    {
        if (_items.Count == 0)
        {
            return;
        }

        BUILD_VERTEX_ARRAY();
        target.Draw(_vertices, new RenderStates(_texture));
    }

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Builds the vertex array from queued sprites.
    /// </summary>
    private void BUILD_VERTEX_ARRAY()
    {
        _vertices.Clear();

        foreach (BatchItem item in _items)
        {
            (Vector2f p, Vector2f s, System.Single rot, IntRect src, Color col, Vector2f org) = (
                item.Position,
                item.Scale,
                item.Rotation,
                item.SourceRect,
                item.Color,
                item.Origin);

            // Calculate corners relative to origin.
            Vector2f size = new(src.Width * s.X, src.Height * s.Y);

            // 4 corners: TL, TR, BR, BL (origin is custom)
            Vector2f[] corners =
            [
                new(-org.X, -org.Y),                        // Top-left
                new(size.X - org.X, -org.Y),                // Top-right
                new(size.X - org.X, size.Y - org.Y),        // Bottom-right
                new(-org.X, size.Y - org.Y)                 // Bottom-left
            ];

            System.Single rad = rot * (System.Single)System.Math.PI / 180f;

            // Rotate and translate corners
            if (rot != 0)
            {
                System.Single cos = (System.Single)System.Math.Cos(rad);
                System.Single sin = (System.Single)System.Math.Sin(rad);
                for (System.Int32 i = 0; i < 4; ++i)
                {
                    System.Single x = corners[i].X, y = corners[i].Y;
                    corners[i].X = (x * cos) - (y * sin);
                    corners[i].Y = (x * sin) + (y * cos);
                }
            }
            for (System.Int32 i = 0; i < 4; ++i)
            {
                corners[i] += p;
            }

            // Texture coordinates
            Vector2f[] texCoords =
            [
                new(src.Left, src.Top),                             // TL
                new(src.Left + src.Width, src.Top),                 // TR
                new(src.Left + src.Width, src.Top + src.Height),    // BR
                new(src.Left, src.Top + src.Height)                 // BL
            ];

            // Append 4 vertex making a quad
            for (System.Int32 i = 0; i < 4; ++i)
            {
                _vertices.Append(new Vertex(corners[i], col, texCoords[i]));
            }
        }
    }

    #endregion Private Methods
}