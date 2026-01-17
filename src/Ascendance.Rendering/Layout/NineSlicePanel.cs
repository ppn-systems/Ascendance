// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Layout;

/// <summary>
/// Simple 9-slice panel for crisp, scalable UI frames.
/// Corners unscaled, edges scale in one axis, center in both.
/// </summary>
public sealed class NineSlicePanel : Drawable, System.IDisposable
{
    #region Fields

    private System.Boolean _dirty = true;
    private System.Boolean _disposed;

    private readonly Sprite[] _parts = new Sprite[9];

    #endregion Fields

    #region Properties

    public Texture Texture { get; }

    /// <summary>
    /// Raised when layout changed.
    /// </summary>
    public event System.Action OnLayoutChanged;

    public Vector2f Size { get; private set; }

    public Thickness Border { get; private set; }

    public Vector2f Position { get; private set; }

    public IntRect SourceRect { get; private set; }

    #endregion Properties

    #region Constructor

    public NineSlicePanel(Texture texture, Thickness border, IntRect sourceRect = default)
    {
        Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
        if (border.Left < 0 || border.Top < 0 || border.Right < 0 || border.Bottom < 0)
        {
            throw new System.ArgumentException("Border thickness must be >= 0.", nameof(border));
        }

        Border = border;
        SourceRect = sourceRect == default
            ? new IntRect(0, 0, (System.Int32)texture.Size.X, (System.Int32)texture.Size.Y)
            : sourceRect;

        if (SourceRect.Width < Border.Left + Border.Right || SourceRect.Height < Border.Top + Border.Bottom)
        {
            throw new System.ArgumentException("SourceRect must be large enough to fit border.", nameof(sourceRect));
        }

        for (System.Int32 i = 0; i < 9; i++)
        {
            _parts[i] = new Sprite(Texture);
        }

        Position = default;
        Size = new Vector2f(SourceRect.Width, SourceRect.Height);
        _dirty = true;
    }

    #endregion Constructor

    #region APIs

    public NineSlicePanel SetPosition(Vector2f pos)
    {
        if (pos != Position)
        {
            Position = pos;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetSize(Vector2f size)
    {
        if (size.X < Border.Left + Border.Right || size.Y < Border.Top + Border.Bottom)
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

    public NineSlicePanel SetSourceRect(IntRect rect)
    {
        if (rect.Width < Border.Left + Border.Right || rect.Height < Border.Top + Border.Bottom)
        {
            throw new System.ArgumentException("SourceRect too small for set borders.", nameof(rect));
        }

        if (rect != SourceRect)
        {
            SourceRect = rect;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetBorder(Thickness border)
    {
        if (border.Left < 0 || border.Top < 0 || border.Right < 0 || border.Bottom < 0)
        {
            throw new System.ArgumentException("Border thickness must be >= 0.", nameof(border));
        }

        if (!border.Equals(Border))
        {
            Border = border;
            _dirty = true;
        }
        return this;
    }

    public NineSlicePanel SetColor(Color color)
    {
        foreach (var part in _parts)
        {
            part.Color = color;
        }

        return this;
    }

    public Color GetColor() => _parts.Length > 0 ? _parts[0].Color : Color.White;

    /// <summary>For legacy RenderObject style.</summary>
    public void Render(RenderTarget target)
    {
        if (_dirty)
        {
            Layout();
        }

        for (var i = 0; i < 9; i++)
        {
            target.Draw(_parts[i]);
        }
    }

    /// <summary>
    /// Checks if a point is inside this panel.
    /// </summary>
    public System.Boolean Contains(Vector2f point)
    {
        return point.X >= Position.X &&
               point.X <= Position.X + Size.X &&
               point.Y >= Position.Y &&
               point.Y <= Position.Y + Size.Y;
    }

    /// <summary>
    /// Recompute 9 slices geometry.
    /// </summary>
    public void Layout()
    {
        System.Int32 L = Border.Left, T = Border.Top, R = Border.Right, B = Border.Bottom;
        System.Single minW = L + R, minH = T + B;

        System.Single x = (System.Single)System.Math.Floor(Position.X);
        System.Single y = (System.Single)System.Math.Floor(Position.Y);
        System.Single w = System.Math.Max(Size.X, minW);
        System.Single h = System.Math.Max(Size.Y, minH);

        System.Int32 sx = SourceRect.Left, sy = SourceRect.Top, sw = SourceRect.Width, sh = SourceRect.Height;

        var src = new IntRect[9];
        src[0] = new IntRect(sx, sy, L, T);
        src[2] = new IntRect(sx + sw - R, sy, R, T);
        src[6] = new IntRect(sx, sy + sh - B, L, B);
        src[8] = new IntRect(sx + sw - R, sy + sh - B, R, B);
        src[1] = new IntRect(sx + L, sy, sw - L - R, T);
        src[3] = new IntRect(sx, sy + T, L, sh - T - B);
        src[5] = new IntRect(sx + sw - R, sy + T, R, sh - T - B);
        src[7] = new IntRect(sx + L, sy + sh - B, sw - L - R, B);
        src[4] = new IntRect(sx + L, sy + T, sw - L - R, sh - T - B);

        System.Single Lw = L, Tw = T, Rw = R, Bw = B;

        var dst = new FloatRect[9];
        dst[0] = new FloatRect(x, y, Lw, Tw);
        dst[2] = new FloatRect(x + w - Rw, y, Rw, Tw);
        dst[6] = new FloatRect(x, y + h - Bw, Lw, Bw);
        dst[8] = new FloatRect(x + w - Rw, y + h - Bw, Rw, Bw);
        dst[1] = new FloatRect(x + Lw, y, w - Lw - Rw, Tw);
        dst[3] = new FloatRect(x, y + Tw, Lw, h - Tw - Bw);
        dst[5] = new FloatRect(x + w - Rw, y + Tw, Rw, h - Tw - Bw);
        dst[7] = new FloatRect(x + Lw, y + h - Bw, w - Lw - Rw, Bw);
        dst[4] = new FloatRect(x + Lw, y + Tw, w - Lw - Rw, h - Tw - Bw);

        for (System.Int32 i = 0; i < 9; i++)
        {
            Sprite s = _parts[i];
            s.TextureRect = src[i];
            s.Position = new Vector2f(dst[i].Left, dst[i].Top);

            System.Single dw = System.Math.Max(0f, dst[i].Width);
            System.Single dh = System.Math.Max(0f, dst[i].Height);
            System.Single swp = System.Math.Max(1, src[i].Width);
            System.Single shp = System.Math.Max(1, src[i].Height);

            s.Scale = new Vector2f(dw / swp, dh / shp);
        }
        _dirty = false;
        OnLayoutChanged?.Invoke();
    }

    public void Draw(RenderTarget target, RenderStates states)
    {
        if (_dirty)
        {
            Layout();
        }

        for (var i = 0; i < 9; i++)
        {
            target.Draw(_parts[i], states);
        }
    }

    #endregion APIs

    #region IDisposable Support

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var sprite in _parts)
            {
                sprite?.Dispose();
            }

            _disposed = true;
        }
    }

    #endregion IDisposable Support
}