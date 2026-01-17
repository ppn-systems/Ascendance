// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Layout;

/// <summary>
/// High-performance 9-slice panel using a single VertexArray draw call.
/// </summary>
public sealed class NineSlicePanel : Drawable
{
    #region Constants

    private const System.Int32 SliceCount = 9;
    private const System.Int32 VerticesPerSlice = 4;

    #endregion Constants

    #region Fields

    private readonly VertexArray _vertexArray;

    private System.Boolean _dirty;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the texture used by this panel.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the current size of the panel in pixels.
    /// </summary>
    public Vector2f Size { get; private set; }

    /// <summary>
    /// Gets the current position of the panel.
    /// </summary>
    public Vector2f Position { get; private set; }

    /// <summary>
    /// Gets the border (in pixels) separating the slices.
    /// </summary>
    public Thickness BorderThickness { get; private set; }

    /// <summary>
    /// Gets the region from the texture to use for slicing.
    /// </summary>
    public IntRect TextureSourceRect { get; private set; }

    /// <summary>
    /// Gets the tint color applied to the panel.
    /// </summary>
    public Color TintColor { get; private set; } = Color.White;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="NineSlicePanel"/> class.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    /// <param name="border">The border thickness for the 9-slice (pixels).</param>
    /// <param name="sourceRect">Optional. The source rectangle from the texture, or default for full texture.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="texture"/> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown if border or sourceRect are invalid.</exception>
    public NineSlicePanel(
        Texture texture,
        Thickness border,
        IntRect sourceRect = default)
    {
        this.Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));

        ValidateBorderThickness(border);
        this.BorderThickness = border;

        this.TextureSourceRect = sourceRect == default
            ? new IntRect(0, 0, (System.Int32)texture.Size.X, (System.Int32)texture.Size.Y)
            : sourceRect;

        ValidateTextureSourceRect(TextureSourceRect, BorderThickness);

        this.Size = new Vector2f(TextureSourceRect.Width, TextureSourceRect.Height);

        _dirty = true;
        _vertexArray = new(PrimitiveType.Quads, SliceCount * VerticesPerSlice);
    }

    #endregion Constructor

    #region Fluent API

    /// <summary>
    /// Sets the panel's position in screen coordinates.
    /// </summary>
    /// <param name="position">The new position.</param>
    /// <returns>This panel instance (for chaining).</returns>
    public NineSlicePanel SetPosition(Vector2f position)
    {
        if (position != Position)
        {
            Position = position;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the panel's size in pixels.
    /// </summary>
    /// <param name="size">The new size.</param>
    /// <returns>This panel instance (for chaining).</returns>
    /// <exception cref="System.ArgumentException">Thrown if size is smaller than borders.</exception>
    public NineSlicePanel SetSize(Vector2f size)
    {
        if (size.X < BorderThickness.Left + BorderThickness.Right ||
            size.Y < BorderThickness.Top + BorderThickness.Bottom)
        {
            throw new System.ArgumentException("Size is too small for borders.", nameof(size));
        }

        if (size != Size)
        {
            Size = size;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the border thickness for slicing this panel.
    /// </summary>
    /// <param name="border">The border thickness.</param>
    /// <returns>This panel instance (for chaining).</returns>
    /// <exception cref="System.ArgumentException">Thrown if <paramref name="border"/> is invalid.</exception>
    public NineSlicePanel SetBorderThickness(Thickness border)
    {
        ValidateBorderThickness(border);

        if (!border.Equals(BorderThickness))
        {
            BorderThickness = border;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the source rectangle from the texture for this panel.
    /// </summary>
    /// <param name="rect">The source texture rectangle.</param>
    /// <returns>This panel instance (for chaining).</returns>
    /// <exception cref="System.ArgumentException">Thrown if <paramref name="rect"/> is too small for borders.</exception>
    public NineSlicePanel SetTextureSourceRect(IntRect rect)
    {
        ValidateTextureSourceRect(rect, BorderThickness);

        if (rect != TextureSourceRect)
        {
            TextureSourceRect = rect;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the panel color tint.
    /// </summary>
    /// <param name="color">The panel color.</param>
    /// <returns>This panel instance (for chaining).</returns>
    public NineSlicePanel SetTintColor(Color color)
    {
        if (color != TintColor)
        {
            TintColor = color;
            _dirty = true;
        }
        return this;
    }

    #endregion Fluent API

    #region Layout

    private void RebuildVertices()
    {
        System.Int32 L = BorderThickness.Left;
        System.Int32 T = BorderThickness.Top;
        System.Int32 R = BorderThickness.Right;
        System.Int32 B = BorderThickness.Bottom;

        System.Single x = System.MathF.Floor(Position.X);
        System.Single y = System.MathF.Floor(Position.Y);
        System.Single w = System.MathF.Max(Size.X, L + R);
        System.Single h = System.MathF.Max(Size.Y, T + B);

        System.Int32 sx = TextureSourceRect.Left;
        System.Int32 sy = TextureSourceRect.Top;
        System.Int32 sw = TextureSourceRect.Width;
        System.Int32 sh = TextureSourceRect.Height;

        // Source rects (texture space)
        IntRect[] src =
        [
            new(sx, sy, L, T),
            new(sx + L, sy, sw - L - R, T),
            new(sx + sw - R, sy, R, T),

            new(sx, sy + T, L, sh - T - B),
            new(sx + L, sy + T, sw - L - R, sh - T - B),
            new(sx + sw - R, sy + T, R, sh - T - B),

            new(sx, sy + sh - B, L, B),
            new(sx + L, sy + sh - B, sw - L - R, B),
            new(sx + sw - R, sy + sh - B, R, B),
        ];

        // Destination rects (screen space)
        FloatRect[] dst =
        [
            new(x, y, L, T),
            new(x + L, y, w - L - R, T),
            new(x + w - R, y, R, T),

            new(x, y + T, L, h - T - B),
            new(x + L, y + T, w - L - R, h - T - B),
            new(x + w - R, y + T, R, h - T - B),

            new(x, y + h - B, L, B),
            new(x + L, y + h - B, w - L - R, B),
            new(x + w - R, y + h - B, R, B),
        ];

        for (System.Int32 i = 0; i < SliceCount; i++)
        {
            WriteSliceQuad(i, dst[i], src[i]);
        }

        _dirty = false;
    }

    private void WriteSliceQuad(System.Int32 index, FloatRect dst, IntRect src)
    {
        System.Int32 v = index * VerticesPerSlice;

        System.Single dx = dst.Left;
        System.Single dy = dst.Top;
        System.Single dw = System.MathF.Max(0, dst.Width);
        System.Single dh = System.MathF.Max(0, dst.Height);

        System.Single sx = src.Left;
        System.Single sy = src.Top;
        System.Single sw = System.MathF.Max(1, src.Width);
        System.Single sh = System.MathF.Max(1, src.Height);

        _vertexArray[(System.UInt32)(v + 0)] = new Vertex(new(dx, dy), TintColor, new(sx, sy));
        _vertexArray[(System.UInt32)(v + 1)] = new Vertex(new(dx + dw, dy), TintColor, new(sx + sw, sy));
        _vertexArray[(System.UInt32)(v + 2)] = new Vertex(new(dx + dw, dy + dh), TintColor, new(sx + sw, sy + sh));
        _vertexArray[(System.UInt32)(v + 3)] = new Vertex(new(dx, dy + dh), TintColor, new(sx, sy + sh));
    }

    #endregion Layout

    #region Draw

    /// <summary>
    /// Draws the panel to the target using the specified render states.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="states">The render states.</param>
    public void Draw(RenderTarget target, RenderStates states)
    {
        if (_dirty)
        {
            RebuildVertices();
        }

        states.Texture = Texture;
        target.Draw(_vertexArray, states);
    }

    #endregion Draw

    #region Validation

    private static void ValidateBorderThickness(Thickness border)
    {
        if (border.Left < 0 || border.Top < 0 ||
            border.Right < 0 || border.Bottom < 0)
        {
            throw new System.ArgumentException("Border thickness must be >= 0.", nameof(border));
        }
    }

    private static void ValidateTextureSourceRect(IntRect rect, Thickness border)
    {
        if (rect.Width < border.Left + border.Right ||
            rect.Height < border.Top + border.Bottom)
        {
            throw new System.ArgumentException("SourceRect is too small for borders.", nameof(rect));
        }
    }

    #endregion Validation
}