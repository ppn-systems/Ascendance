// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Internal.Input;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Logging.Extensions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Engine;

/// <summary>
/// Central static class for managing the main game window, rendering loop, and core events.
/// </summary>
public static class GraphicsEngine
{
    #region Fields

    private static readonly RenderWindow _window;
    private static readonly System.UInt32 _foregroundFps;
    private static readonly System.UInt32 _backgroundFps;

    private static System.Boolean _isFocused;
    private static System.Boolean _renderCacheDirty;
    private static System.Collections.Generic.List<RenderObject> _renderObjectCache;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets whether debug mode is enabled.
    /// </summary>
    public static System.Boolean IsDebugMode { get; private set; }

    /// <summary>
    /// Gets current window size.
    /// </summary>
    public static Vector2u ScreenSize { get; private set; }

    /// <summary>
    /// Gets application graphics configuration.
    /// </summary>
    public static GraphicsConfig GraphicsConfig { get; }

    /// <summary>
    /// Sets a user-defined per-frame update handler.
    /// </summary>
    public static System.Action<System.Single> FrameUpdate { get; set; }

    #endregion Properties

    #region Constructor

    static GraphicsEngine()
    {
        GraphicsConfig = ConfigurationManager.Instance.Get<GraphicsConfig>();
        ScreenSize = new Vector2u(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);

        _isFocused = true;
        _backgroundFps = 15;
        _renderObjectCache = [];
        _renderCacheDirty = true;
        _foregroundFps = GraphicsConfig.FrameLimit > 0 ? GraphicsConfig.FrameLimit : 60;

        ContextSettings ctx = new()
        {
            AntialiasingLevel = 0,
            DepthBits = 0,
            StencilBits = 0
        };

        _window = new RenderWindow(
            new VideoMode(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight),
            GraphicsConfig.Title, Styles.Titlebar | Styles.Close, ctx
        );

        // Window events
        _window.Closed += (_, _) => _window.Close();
        _window.LostFocus += (_, _) => HANDLE_FOCUS_CHANGED(false);
        _window.GainedFocus += (_, _) => HANDLE_FOCUS_CHANGED(true);
        _window.Resized += (_, e) => ScreenSize = new Vector2u(e.Width, e.Height);

        // Prefer VSync if available
        if (GraphicsConfig.VSync)
        {
            _window.SetVerticalSyncEnabled(true);
        }
        else
        {
            _window.SetFramerateLimit(_foregroundFps);
        }
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Enables or disables debug mode.
    /// </summary>
    public static void DebugMode() => IsDebugMode = !IsDebugMode;

    /// <summary>
    /// Sets the icon for the game window.
    /// </summary>
    public static void SetWindowIcon(Image image) => _window.SetIcon(image.Size.X, image.Size.Y, image.Pixels);

    /// <summary>
    /// Starts the main game window loop.
    /// </summary>
    public static void Run()
    {
        const System.Single targetDelta = 1f / 60f;

        System.Single accumulator = 0f;
        Clock clock = InstanceManager.Instance.GetOrCreateInstance<Clock>();

        SceneManager.InitializeScenes();
        System.Threading.Thread.Sleep(20);

        try
        {
            while (_window.IsOpen)
            {
                _window.DispatchEvents();

                System.Single frameDelta = clock.Restart().AsSeconds();
                if (frameDelta > 0.25f)
                {
                    frameDelta = 0.25f;
                }

                accumulator += frameDelta;
                while (accumulator >= targetDelta)
                {
                    UPDATE_FRAME(targetDelta);
                    accumulator -= targetDelta;
                }

                _window.Clear();
                GraphicsEngine.DRAW(_window);
                _window.Display();

                if (!_isFocused)
                {
                    if (!GraphicsConfig.VSync)
                    {
                        _window.SetFramerateLimit(_backgroundFps);
                    }

                    System.Threading.Thread.Sleep(2);
                }
                else
                {
                    if (!GraphicsConfig.VSync)
                    {
                        _window.SetFramerateLimit(_foregroundFps);
                    }
                }
            }

            _window.Dispose();
        }
        catch (System.Exception ex)
        {
            NLogixFx.Error($"Unhandled exception in main game loop: {ex}", source: "GraphicsEngine");
        }
        finally
        {
            _window.Dispose();
            try { MusicManager.Dispose(); } catch { }
        }
    }

    /// <summary>
    /// Closes the game window and disposes of systems.
    /// </summary>
    public static void Shutdown()
    {
        _window.Close();

        try
        {
            MusicManager.Dispose();
        }
        catch { /* intentionally ignore */ }
    }

    #endregion Methods

    #region Private Methods

    /// <summary>
    /// Per-frame: updates input, scenes, and user code.
    /// </summary>
    private static void UPDATE_FRAME(System.Single deltaTime)
    {
        FrameUpdate?.Invoke(deltaTime);

        MouseManager.Instance.Update(_window);
        KeyboardManager.Instance.Update();
        InputTimeline.Instance.Update();

        SceneManager.ProcessSceneChange();
        SceneManager.ProcessPendingDestroy();
        SceneManager.ProcessPendingSpawn();
        SceneManager.UpdateSceneObjects(deltaTime);
    }

    /// <summary>
    /// Draws all visible scene objects, sorted by Z-index.
    /// </summary>
    private static void DRAW(RenderTarget target)
    {
        if (_renderCacheDirty)
        {
            _renderObjectCache = [.. SceneManager.GetAllObjectsOfType<RenderObject>()];
            _renderObjectCache.Sort(RenderObject.CompareZIndex);
            _renderCacheDirty = false;
        }

        foreach (RenderObject obj in _renderObjectCache)
        {
            if (obj.IsEnabled && obj.IsVisible)
            {
                obj.Draw(target);
            }
        }
    }

    /// <summary>
    /// Handles application focus changes (foreground/background).
    /// </summary>
    private static void HANDLE_FOCUS_CHANGED(System.Boolean focused)
    {
        _isFocused = focused;
        if (!GraphicsConfig.VSync)
        {
            _window.SetFramerateLimit(focused ? _foregroundFps : _backgroundFps);
        }
    }

    #endregion Private Methods
}