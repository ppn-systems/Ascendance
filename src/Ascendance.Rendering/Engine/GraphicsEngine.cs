// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.Time;
using Ascendance.Shared.Abstractions;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Framework.Injection.DI;
using Nalix.Logging.Extensions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Engine;

/// <summary>
/// Central static class for managing the main game window, rendering loop, and core events.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IsRunning={IsRunning}, DebugMode={IsDebugMode}, WindowTitle={RenderWindow?.Title}")]
public class GraphicsEngine : SingletonBase<GraphicsEngine>, IUpdatable
{
    #region Constants

    private const System.Int32 INITIAL_SLEEP_MS = 20;
    private const System.Int32 LOW_POWER_SLEEP_MS = 2;
    private const System.Int32 DEFAULT_BACKGROUND_FPS = 15;

    #endregion Constants

    #region Fields

    private readonly System.UInt32 _foregroundFps;
    private readonly System.UInt32 _backgroundFps;

    private System.Boolean _isFocused;
    private System.Single _lastLogicMs;
    private System.Single _lastRenderMs;
    private System.Boolean _renderCacheDirty;
    private System.Collections.Generic.IReadOnlyList<RenderObject> _renderObjectCache;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Window used for rendering.
    /// </summary>
    public readonly RenderWindow RenderWindow;

    /// <summary>
    /// Gets application graphics configuration.
    /// </summary>
    public static GraphicsConfig GraphicsConfig { get; }

    /// <summary>
    /// Gets current window size.
    /// </summary>
    public static Vector2u ScreenSize { get; private set; }

    /// <summary>
    /// Gets whether debug mode is enabled.
    /// </summary>
    public System.Boolean IsDebugMode { get; private set; }

    /// <summary>
    /// Sets a user-defined per-frame render handler.
    /// </summary>
    public event System.Action<RenderTarget> FrameRender;

    /// <summary>
    /// Sets a user-defined per-frame update handler.
    /// </summary>
    public event System.Action<System.Single> FrameUpdate;

    /// <summary>
    /// Window running state.
    /// </summary>
    public System.Boolean IsRunning => this.RenderWindow.IsOpen;

    /// <summary>
    /// Last logic update time in milliseconds.
    /// </summary>
    public System.Single LogicUpdateMilliseconds => _lastLogicMs;

    /// <summary>
    /// Last render time in milliseconds.
    /// </summary>
    public System.Single RenderFrameMilliseconds => _lastRenderMs;

    /// <summary>
    /// Gets the number of active and visible render objects.
    /// </summary>
    /// <returns>The count of objects that are enabled and visible.</returns>
    public System.Int32 ActiveObjectCount => System.Linq.Enumerable.Count(_renderObjectCache, obj => obj.IsEnabled && obj.IsVisible);

    #endregion Properties

    #region Constructor

    static GraphicsEngine()
    {
        GraphicsConfig = ConfigurationManager.Instance.Get<GraphicsConfig>();
        ScreenSize = new Vector2u(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsEngine"/> class, configures the main window and sets up event handlers.
    /// </summary>
    public GraphicsEngine()
    {
        _lastLogicMs = 0f;
        _isFocused = true;
        _lastRenderMs = 0f;
        _renderObjectCache = [];
        _renderCacheDirty = true;
        _backgroundFps = DEFAULT_BACKGROUND_FPS;
        _foregroundFps = GraphicsConfig.FrameLimit > 0 ? GraphicsConfig.FrameLimit : 60;

        ContextSettings ctx = new()
        {
            DepthBits = 0,
            StencilBits = 0,
            AntialiasingLevel = 0
        };

        this.RenderWindow = new RenderWindow(
            new VideoMode(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight),
            GraphicsConfig.Title, Styles.Titlebar | Styles.Close, ctx
        );

        // Window events
        this.IsDebugMode = false;
        this.RenderWindow.Closed += (_, _) => this.CLOSE_RENDER_WINDOW();
        this.RenderWindow.LostFocus += (_, _) => this.HANDLE_FOCUS_CHANGED(false);
        this.RenderWindow.GainedFocus += (_, _) => this.HANDLE_FOCUS_CHANGED(true);
        this.RenderWindow.Resized += (_, e) => ScreenSize = new Vector2u(e.Width, e.Height);

        // Prefer VSync if available
        if (GraphicsConfig.VSync)
        {
            this.RenderWindow.SetVerticalSyncEnabled(true);
        }
        else
        {
            this.RenderWindow.SetFramerateLimit(_foregroundFps);
        }
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Enables or disables debug mode.
    /// </summary>
    public void DebugMode() => this.IsDebugMode = !this.IsDebugMode;

    /// <summary>
    /// Sets the icon for the game window.
    /// </summary>
    /// <param name="image">The icon image to use for the window.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="image"/> is null or has no pixel data.</exception>
    public void SetIcon(Image image)
    {
        if (image == null || image.Pixels == null)
        {
            NLogixFx.Error(message: "SetIcon called with null or image.Pixels is null", source: "GraphicsEngine");
            throw new System.ArgumentNullException(nameof(image));
        }

        this.RenderWindow.SetIcon(image.Size.X, image.Size.Y, image.Pixels);
        NLogixFx.Debug(message: $"Window icon set (size: {image.Size.X}x{image.Size.Y})", source: "GraphicsEngine");
    }

    /// <summary>
    /// Starts the main game window loop.
    /// </summary>
    /// <param name="strings">Optional command-line arguments (unused).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public void Launch(System.String[] strings = null)
    {
        const System.Single MAX_ACCUMULATOR = 0.25f;
        const System.Int32 MAX_UPDATES_PER_FRAME = 5;
        System.Single accumulator = 0f;
        TimeService time = InstanceManager.Instance.GetOrCreateInstance<TimeService>();

        SceneManager.Instance.InitializeScenes();
        SceneManager.Instance.SceneChanged += (sender, args) => _renderCacheDirty = true;
        SceneManager.Instance.ObjectsModified += (sender, args) => _renderCacheDirty = true;

        System.Threading.Thread.Sleep(INITIAL_SLEEP_MS);

        try
        {
            NLogixFx.Info(message: "Game loop started.", source: "GraphicsEngine");
            while (this.RenderWindow.IsOpen)
            {
                this.RenderWindow.DispatchEvents();

                time.Update();
                Camera2D.Instance.Apply(RenderWindow);

                System.Int32 updateIterations = 0;
                accumulator += System.Math.Min(time.Current.DeltaTime, MAX_ACCUMULATOR);

                while (accumulator >= time.FixedDeltaTime &&
                       updateIterations < MAX_UPDATES_PER_FRAME)
                {
                    this.Update(time.FixedDeltaTime);
                    accumulator -= time.FixedDeltaTime;
                    updateIterations++;
                }

                // If we hit the max, reset accumulator
                if (updateIterations >= MAX_UPDATES_PER_FRAME)
                {
                    NLogixFx.Warn(message: "Frame took too long, resetting accumulator", source: "GraphicsEngine");
                    accumulator = 0f;
                }

                this.RenderWindow.Clear();
                this.UPDATE_DRAW(this.RenderWindow);
                this.RenderWindow.Display();

                if (!_isFocused)
                {
                    if (!GraphicsConfig.VSync)
                    {
                        this.RenderWindow.SetFramerateLimit(_backgroundFps);
                    }

                    System.Threading.Thread.Sleep(LOW_POWER_SLEEP_MS);
                }
                else
                {
                    if (!GraphicsConfig.VSync)
                    {
                        this.RenderWindow.SetFramerateLimit(_foregroundFps);
                    }
                }
            }

            this.RenderWindow.Dispose();
            NLogixFx.Info(message: "Game window closed, exiting main loop.", source: "GraphicsEngine");
        }
        catch (System.Exception ex)
        {
            NLogixFx.Error($"Unhandled exception in main game loop: {ex}", source: "GraphicsEngine");
        }
        finally
        {
            this.RenderWindow.Dispose();
            NLogixFx.Debug(message: "Window disposed in finally block.", source: "GraphicsEngine");

            try
            {
                MusicManager.Dispose();
            }
            catch (System.Exception ex)
            {
                NLogixFx.Warn(message: $"Exception when disposing MusicManager: {ex?.Message}", source: "GraphicsEngine");
            }
        }
    }

    /// <summary>
    /// Closes the game window and disposes of systems.
    /// </summary>
    public void Shutdown()
    {
        RenderWindow.Close();
        NLogixFx.Info(message: "Shutdown called: RenderWindow.Close() invoked.", source: "GraphicsEngine");

        try
        {
            MusicManager.Dispose();
        }
        catch (System.Exception ex)
        {
            NLogixFx.Warn(message: $"Exception when disposing MusicManager during shutdown: {ex?.Message}", source: "GraphicsEngine");
        }
    }

    /// <summary>
    /// Per-frame method: updates input, scenes, and user code.
    /// </summary>
    /// <param name="deltaTime">The time in seconds since the previous update.</param>
    public virtual void Update(System.Single deltaTime)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        this.FrameUpdate?.Invoke(deltaTime);

        KeyboardManager.Instance.Update();
        MouseManager.Instance.Update(RenderWindow);

        SceneManager.Instance.ProcessSceneChange();
        SceneManager.Instance.ProcessPendingSpawn();
        SceneManager.Instance.ProcessPendingDestroy();
        SceneManager.Instance.Update(deltaTime);

        sw.Stop();
        _lastLogicMs = (System.Single)sw.Elapsed.TotalMilliseconds;
    }

    #endregion Methods

    #region Private Methods

    private void CLOSE_RENDER_WINDOW()
    {
        this.RenderWindow.Close();
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Draws all visible scene objects, sorted by Z-index.
    /// </summary>
    /// <param name="target">The render target to draw scene objects to.</param>
    private void UPDATE_DRAW(RenderTarget target)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        if (_renderCacheDirty)
        {
            System.Collections.Generic.List<RenderObject> cache = [.. SceneManager.Instance.GetActiveObjects<RenderObject>()];
            cache.Sort(RenderObject.CompareZIndex);

            _renderCacheDirty = false;
            _renderObjectCache = cache;

            NLogixFx.Debug(message: $"Render cache rebuilt ({cache.Count} objects).", source: "GraphicsEngine");
        }

        foreach (RenderObject obj in _renderObjectCache)
        {
            if (obj.IsEnabled && obj.IsVisible)
            {
                obj.Draw(target);
            }
        }

        FrameRender?.Invoke(target);

        sw.Stop();
        _lastRenderMs = (System.Single)sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Handles application focus changes (foreground/background).
    /// </summary>
    /// <param name="focused">True if window is in the foreground; otherwise, false.</param>
    private void HANDLE_FOCUS_CHANGED(System.Boolean focused)
    {
        _isFocused = focused;

        if (!GraphicsConfig.VSync)
        {
            this.RenderWindow.SetFramerateLimit(focused ? _foregroundFps : _backgroundFps);
        }

        NLogixFx.Info(message: $"Window focus changed: {(focused ? "Gained" : "Lost")}", source: "GraphicsEngine");
    }

    #endregion Private Methods
}