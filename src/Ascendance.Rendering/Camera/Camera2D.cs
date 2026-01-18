// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Random;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Camera;

/// <summary>
/// Represents a 2D camera for controlling the viewport in a tile-based or scrolling game.
/// </summary>
public class Camera2D
{
    private System.Single _zoom = 1f;
    private System.Single _shakeAmount;

    /// <summary>
    /// Gets the current SFML View used for rendering.
    /// </summary>
    public View SFMLView { get; }

    public FloatRect Bounds { get; set; }

    /// <summary>
    /// Initializes a new Camera2D instance.
    /// </summary>
    /// <param name="center">Initial center position of the camera.</param>
    /// <param name="size">Initial viewport size (screen, usually in pixels).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Camera2D(Vector2f center, Vector2f size) => this.SFMLView = new View(center, size);

    public void ClampToBounds()
    {
        var half = SFMLView.Size / 2f;
        var min = new Vector2f(Bounds.Left + half.X, Bounds.Top + half.Y);
        var max = new Vector2f(Bounds.Left + Bounds.Width - half.X, Bounds.Top + Bounds.Height - half.Y);

        var center = SFMLView.Center;
        center.X = System.Math.Max(min.X, System.Math.Min(max.X, center.X));
        center.Y = System.Math.Max(min.Y, System.Math.Min(max.Y, center.Y));
        SFMLView.Center = center;
    }

    public void Follow(Vector2f target, System.Single smooth = 0.1f)
    {
        var delta = (target - SFMLView.Center) * smooth;
        Move(delta);
    }

    public void Shake(System.Single amount) => _shakeAmount = amount;

    public void UpdateShake()
    {
        if (_shakeAmount > 0)
        {
            Vector2f offset = new(
                ((System.Single)Csprng.NextDouble() - 0.5f) * _shakeAmount,
                ((System.Single)Csprng.NextDouble() - 0.5f) * _shakeAmount);

            _shakeAmount *= 0.9f; // giảm dần hiệu ứng
            SFMLView.Center += offset;
        }
    }

    /// <summary>
    /// Sets the center position of the camera.
    /// </summary>
    /// <param name="center">The new center position.</param>
    public void SetCenter(Vector2f center) => this.SFMLView.Center = center;

    /// <summary>
    /// Moves the camera by the given offset.
    /// </summary>
    /// <param name="offset">The amount to move (shift) the camera.</param>
    public void Move(Vector2f offset) => this.SFMLView.Center += offset;

    /// <summary>
    /// Zooms the camera by a specific factor.
    /// </summary>
    /// <param name="factor">Zoom factor, greater than zero (e.g. 1.2 = zoom in, 0.8 = zoom out).</param>
    public void Zoom(System.Single factor)
    {
        if (factor <= 0)
        {
            return;
        }

        this.SFMLView.Zoom(factor);
        _zoom *= factor;
    }

    /// <summary>
    /// Sets the camera zoom directly.
    /// </summary>
    /// <param name="zoom">Absolute zoom value (1 = normal, 2 = 2x zoom out).</param>
    public void SetZoom(System.Single zoom)
    {
        if (zoom <= 0)
        {
            return;
        }
        // Reset to original size then zoom so scaling is correct
        this.SFMLView.Zoom(zoom / _zoom);
        _zoom = zoom;
    }

    /// <summary>
    /// Gets the current zoom value.
    /// </summary>
    public System.Single GetZoom() => _zoom;

    /// <summary>
    /// Sets the camera viewport size (e.g. for window resize).
    /// </summary>
    /// <param name="size">The new size of viewport.</param>
    public void SetSize(Vector2f size) => this.SFMLView.Size = size;

    /// <summary>
    /// Applies this camera view to the target RenderWindow.
    /// </summary>
    /// <param name="window">The target render window.</param>
    public void Apply(RenderWindow window) => window.SetView(this.SFMLView);

    /// <summary>
    /// Resets the camera to default zoom (1x) and center.
    /// </summary>
    /// <param name="center">New center position.</param>
    /// <param name="size">New viewport size.</param>
    public void Reset(Vector2f center, Vector2f size)
    {
        this.SFMLView.Center = center;
        this.SFMLView.Size = size;
        SetZoom(1f);
    }
}