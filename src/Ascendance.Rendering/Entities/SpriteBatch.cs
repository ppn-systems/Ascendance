using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Entities;

/// <summary>
/// Efficiently draws multiple sprites in a single draw call with support for custom position, scale, rotation, color, and animation frames on a texture atlas.
/// </summary>
public class SpriteBatch
{
    #region Fields and Structs

    private readonly Texture _texture;
    private readonly VertexArray _vertices;
    private readonly System.Collections.Generic.List<BatchItem> _items;

    /// <summary>
    /// Represents a single batched sprite with transform and appearance options.
    /// </summary>
    private struct BatchItem(
        Vector2f pos, IntRect src,
        Color color, Vector2f scale,
        System.Single rot, Vector2f origin)
    {
        public Color Color = color;
        public Vector2f Scale = scale;
        public Vector2f Position = pos;
        public IntRect SourceRect = src;
        public Vector2f Origin = origin;
        public System.Single Rotation = rot;
    }

    #endregion Fields and Structs

    #region Construction

    /// <summary>
    /// Initializes a new SpriteBatch for the given texture atlas.
    /// </summary>
    /// <param name="texture">Texture atlas to be used by the batch.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public SpriteBatch(Texture texture)
    {
        _texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
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
    /// Adds a sprite to the batch with full transform and coloring options.
    /// </summary>
    /// <param name="position">Position of the sprite on screen.</param>
    /// <param name="sourceRect">Rectangle on the texture (animation frame).</param>
    /// <param name="color">Pixel color (tint and alpha). Default white.</param>
    /// <param name="scale">Scaling factor. Default (1,1).</param>
    /// <param name="rotation">Rotation angle in degrees. Default 0.</param>
    /// <param name="origin">Origin within the frame for transform center. Default (0,0).</param>
    public void Add(
        Vector2f position,
        IntRect sourceRect,
        Color? color = null,
        Vector2f? scale = null,
        System.Single rotation = 0f,
        Vector2f? origin = null)
    {
        _items.Add(new BatchItem(
            position,
            sourceRect,
            color ?? Color.White,
            scale ?? new Vector2f(1f, 1f),
            rotation,
            origin ?? new Vector2f(0f, 0f)
        ));
    }

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

            // Calculate corners relative to origin
            Vector2f size = new(src.Width * s.X, src.Height * s.Y);

            // 4 corners before rotation: TL, TR, BR, BL (origin is custom)
            Vector2f[] corners =
            [
                new(-org.X, -org.Y),                    // Top-left
                new(size.X - org.X, -org.Y),            // Top-right
                new(size.X - org.X, size.Y - org.Y),    // Bottom-right
                new(-org.X, size.Y - org.Y)             // Bottom-left
            ];

            System.Single rad = (System.Single)(rot * System.Math.PI / 180.0);

            // Rotate and translate corners
            for (System.Int32 i = 0; i < 4; ++i)
            {
                System.Single x = corners[i].X, y = corners[i].Y;
                if (rot != 0)
                {
                    corners[i].X = (x * (System.Single)System.Math.Cos(rad)) - (y * (System.Single)System.Math.Sin(rad));
                    corners[i].Y = (x * (System.Single)System.Math.Sin(rad)) + (y * (System.Single)System.Math.Cos(rad));
                }
                corners[i] += p;
            }

            // Texture coordinates
            Vector2f[] texCoords =
            [
                new(src.Left, src.Top),                                 // TL
                new(src.Left + src.Width, src.Top),                     // TR
                new(src.Left + src.Width, src.Top + src.Height),        // BR
                new(src.Left, src.Top + src.Height)                     // BL
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