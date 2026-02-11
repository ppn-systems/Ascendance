// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Shared.Abstractions;
using Nalix.Framework.Injection.DI;
using Nalix.Framework.Random;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Camera;

/// <summary>
/// Represents a 2D camera responsible for controlling the visible viewport
/// in a scrolling or tile-based game world.
/// </summary>
/// <remarks>
/// <para>
/// This camera wraps an <see cref="View"/> instance from SFML and provides
/// higher-level operations such as:
/// </para>
/// <list type="bullet">
/// <item>Target following with smoothing</item>
/// <item>World bounds clamping</item>
/// <item>Camera shake effects</item>
/// <item>Zoom management with absolute and relative control</item>
/// </list>
/// <para>
/// Designed to minimize per-frame allocations and be safe for real-time usage.
/// </para>
/// </remarks>
public class Camera2D : SingletonBase<Camera2D>, IUpdatable
{
    #region Fields

    private System.Single _zoom;
    private Vector2f _baseCenter;
    private Vector2f _shakeOffset;
    private System.Single _shakeAmount;
    private readonly System.Single _shakeDecayPerSecond;

    // Backing fields for enable/disable properties
    private System.Boolean _shakeEnabled;
    private System.Boolean _followEnabled;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the underlying SFML <see cref="View"/> used for rendering.
    /// </summary>
    /// <remarks>
    /// This view should be applied to a <see cref="RenderWindow"/>
    /// via <see cref="Update(RenderWindow)"/>.
    /// </remarks>
    public View SFMLView { get; }

    /// <summary>
    /// Gets or sets the world-space bounds that the camera is allowed to move within.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="ClampToBounds"/> to prevent the camera from showing
    /// areas outside the playable world.
    /// </remarks>
    public FloatRect Bounds { get; set; }

    /// <summary>
    /// Enable or disable camera shake. When disabled, shake is cleared and camera center
    /// returns to the base center immediately.
    /// </summary>
    public System.Boolean ShakeEnabled
    {
        get => _shakeEnabled;
        set
        {
            _shakeEnabled = value;
            if (!_shakeEnabled)
            {
                // Clear any active shake and restore center to base center
                _shakeAmount = 0f;
                _shakeOffset = new Vector2f(0f, 0f);
                this.SFMLView.Center = _baseCenter;
            }
        }
    }

    /// <summary>
    /// Enable or disable automatic follow behavior. When disabled, calls to Follow()
    /// will be ignored; manual SetCenter/Move still work.
    /// </summary>
    public System.Boolean FollowEnabled
    {
        get => _followEnabled;
        set
        {
            _followEnabled = value;
            // When disabling follow we do not alter _baseCenter, but ensure view shows current base center
            if (!_followEnabled)
            {
                this.SFMLView.Center = _baseCenter;
            }
        }
    }

    /// <summary>
    /// Enable or disable bounds clamping. When disabled, camera will not be clamped to Bounds.
    /// </summary>
    public System.Boolean ClampEnabled { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class with default settings.
    /// </summary>
    /// <remarks>
    /// Creates a view with center at (0, 0) and size of (GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y).
    /// </remarks>
    public Camera2D() : this(new Vector2f(0f, 0f), new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    /// <param name="center">
    /// Initial center position of the camera in world coordinates.
    /// </param>
    /// <param name="size">
    /// Initial viewport size, typically matching the window resolution (in pixels).
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Camera2D(Vector2f center, Vector2f size)
    {
        _zoom = 1f;
        _baseCenter = center;
        _shakeDecayPerSecond = 3.0f;

        // Enable features by default
        _shakeEnabled = true;
        _followEnabled = true;
        this.ClampEnabled = true;

        this.SFMLView = new View(center, size);
    }

    #endregion Constructor

    #region Public Methods

    public View GetView() => this.SFMLView;

    /// <summary>
    /// Clamps the camera center so that it stays entirely within <see cref="Bounds"/>.
    /// </summary>
    /// <remarks>
    /// This method assumes that <see cref="Bounds"/> is larger than the current view size.
    /// Call this after camera movement (e.g. follow or shake) to enforce limits.
    /// </remarks>
    public void ClampToBounds()
    {
        Vector2f center = this.SFMLView.Center;
        Vector2f half = this.SFMLView.Size / 2f;
        Vector2f min = new(this.Bounds.Left + half.X, this.Bounds.Top + half.Y);
        Vector2f max = new(this.Bounds.Left + this.Bounds.Width - half.X, this.Bounds.Top + this.Bounds.Height - half.Y);

        center.X = System.Math.Max(min.X, System.Math.Min(max.X, center.X));
        center.Y = System.Math.Max(min.Y, System.Math.Min(max.Y, center.Y));
        this.SFMLView.Center = center;
    }

    /// <summary>
    /// Smoothly moves the camera toward a target position.
    /// </summary>
    /// <param name="target">
    /// Target world position for the camera to follow.
    /// </param>
    /// <param name="smooth">
    /// Smoothing factor in the range (0, 1].
    /// Smaller values result in slower, smoother movement.
    /// </param>
    /// <remarks>
    /// This implements simple linear interpolation (LERP).
    /// </remarks>
    public void Follow(Vector2f target, System.Single smooth = 0.1f)
    {
        if (!this.FollowEnabled)
        {
            return;
        }

        _baseCenter += (target - _baseCenter) * smooth;
    }

    /// <summary>
    /// Triggers a camera shake effect.
    /// </summary>
    /// <param name="amount">
    /// Initial shake intensity in world units.
    /// </param>
    /// <remarks>
    /// The shake intensity decays automatically over time.
    /// If shake is disabled, this call has no effect and the camera remains stable.
    /// </remarks>
    public void Shake(System.Single amount)
    {
        if (!this.ShakeEnabled)
        {
            // If disabled, ensure no shake is active and keep center normal
            _shakeAmount = 0f;
            _shakeOffset = new Vector2f(0f, 0f);
            this.SFMLView.Center = _baseCenter;
            return;
        }

        _shakeAmount = System.Math.Max(_shakeAmount, amount);
    }

    /// <summary>
    /// Updates the camera logic (shake, combine base center and shake offset) for a frame.
    /// </summary>
    /// <remarks>
    /// Call this from your game update loop after object positions (player, entities) are updated.
    /// This method does not apply the view to a window; call Update(RenderWindow) (below) to apply.
    /// </remarks>
    /// <param name="deltaTime">Frame delta time in seconds.</param>
    public void Update(System.Single deltaTime)
    {
        // If shake disabled, ensure it's cleared
        if (!this.ShakeEnabled)
        {
            _shakeAmount = 0f;
            _shakeOffset = new Vector2f(0f, 0f);
        }
        else
        {
            // Only compute shake offset if there is some shake amount
            if (_shakeAmount > 0f)
            {
                // generate pseudo-random values in [-0.5, 0.5)
                System.Single rx = (System.Single)Csprng.NextDouble() - 0.5f;
                System.Single ry = (System.Single)Csprng.NextDouble() - 0.5f;
                _shakeOffset = new Vector2f(rx * _shakeAmount, ry * _shakeAmount);

                // decay shake over time (frame-rate independent)
                _shakeAmount -= _shakeDecayPerSecond * deltaTime;
                if (_shakeAmount <= 0f)
                {
                    _shakeAmount = 0f;
                    _shakeOffset = new Vector2f(0f, 0f);
                }
            }
            else
            {
                _shakeOffset = new Vector2f(0f, 0f);
            }
        }

        // Combine base center and shake offset (no accumulation / no drift)
        Vector2f finalCenter = _baseCenter + _shakeOffset;
        this.SFMLView.Center = finalCenter;

        // Optionally clamp the final center to bounds
        if (this.ClampEnabled && this.Bounds.Width > 0 && this.Bounds.Height > 0)
        {
            this.ClampToBounds();
        }
    }

    /// <summary>
    /// Sets the camera center position directly.
    /// </summary>
    /// <param name="center">New center position in world coordinates.</param>
    public void SetCenter(Vector2f center)
    {
        _baseCenter = center;
        // Keep view visually consistent; Update() will combine offsets if any
        this.SFMLView.Center = center;
    }

    /// <summary>
    /// Moves the camera by a given offset.
    /// </summary>
    /// <param name="offset">Movement delta in world units.</param>
    public void Move(Vector2f offset) => _baseCenter += offset;

    /// <summary>
    /// Applies a relative zoom factor to the camera.
    /// </summary>
    /// <param name="factor">
    /// Zoom multiplier (must be greater than zero).
    /// For example: 1.2 = zoom in, 0.8 = zoom out.
    /// </param>
    public void Zoom(System.Single factor)
    {
        if (factor <= 0f)
        {
            return;
        }

        this.SFMLView.Zoom(factor);
        _zoom *= factor;
    }

    /// <summary>
    /// Sets the camera zoom to an absolute value.
    /// </summary>
    /// <param name="zoom">
    /// Absolute zoom value (1 = normal, 2 = 2x zoom out).
    /// </param>
    /// <remarks>
    /// Internally converts the absolute value into a relative SFML zoom.
    /// </remarks>
    public void SetZoom(System.Single zoom)
    {
        if (zoom <= 0f)
        {
            return;
        }

        this.SFMLView.Zoom(zoom / _zoom);
        _zoom = zoom;
    }

    /// <summary>
    /// Gets the current absolute zoom value.
    /// </summary>
    public System.Single GetZoom() => _zoom;

    /// <summary>
    /// Sets the viewport size of the camera.
    /// </summary>
    /// <param name="size">
    /// New viewport size, typically called after a window resize.
    /// </param>
    public void SetSize(Vector2f size) => this.SFMLView.Size = size;

    /// <summary>
    /// Applies this camera view to a render window.
    /// </summary>
    /// <param name="window">Target SFML render window.</param>
    public void Update(RenderWindow window) => window.SetView(this.SFMLView);

    /// <summary>
    /// Resets the camera to a default state.
    /// </summary>
    /// <param name="center">New center position.</param>
    /// <param name="size">New viewport size.</param>
    /// <remarks>
    /// Resets zoom to <c>1.0</c> and clears any accumulated scaling.
    /// </remarks>
    public void Reset(Vector2f center, Vector2f size)
    {
        this.SetZoom(1f);

        this.SFMLView.Size = size;
        this.SFMLView.Center = center;

        _baseCenter = center;
        _shakeAmount = 0f;
        _shakeOffset = new Vector2f(0f, 0f);
    }

    #endregion Public Methods
}