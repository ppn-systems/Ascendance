// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Internal.Input;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.Time;
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
public class GraphicsEngine : SingletonBase<GraphicsEngine>
{
    #region Fields

    private readonly System.UInt32 _foregroundFps;
    private readonly System.UInt32 _backgroundFps;

    private System.Boolean _isFocused;
    private System.Boolean _renderCacheDirty;
    private System.Collections.Generic.List<RenderObject> _renderObjectCache;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets application graphics configuration.
    /// </summary>
    public static GraphicsConfig GraphicsConfig { get; }

    /// <summary>
    /// Gets current window size.
    /// </summary>
    public static Vector2u ScreenSize { get; private set; }

    /// <summary>
    /// Window used for rendering.
    /// </summary>
    public readonly RenderWindow RenderWindow;

    /// <summary>
    /// Gets whether debug mode is enabled.
    /// </summary>
    public System.Boolean IsDebugMode { get; private set; }

    /// <summary>
    /// Sets a user-defined per-frame update handler.
    /// </summary>
    public System.Action<System.Single> FrameUpdate { get; set; }

    /// <summary>
    /// Window running state.
    /// </summary>
    public System.Boolean IsRunning => this.RenderWindow.IsOpen;

    #endregion Properties

    #region Constructor

    static GraphicsEngine()
    {
        GraphicsConfig = ConfigurationManager.Instance.Get<GraphicsConfig>();
        ScreenSize = new Vector2u(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);
    }

    public GraphicsEngine()
    {
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

        this.RenderWindow = new RenderWindow(
            new VideoMode(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight),
            GraphicsConfig.Title, Styles.Titlebar | Styles.Close, ctx
        );

        // Window events
        this.RenderWindow.Closed += (_, _) => this.RenderWindow.Close();
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
    public void SetIcon(Image image)
    {
        if (image == null || image.Pixels == null)
        {
            throw new System.ArgumentNullException(nameof(image));
        }

        this.RenderWindow.SetIcon(image.Size.X, image.Size.Y, image.Pixels);
    }

    /// <summary>
    /// Starts the main game window loop.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public void Launch(System.String[] strings = null)
    {
        System.Single accumulator = 0f;
        SceneManager.Instance.InitializeScenes();
        TimeService time = InstanceManager.Instance.GetOrCreateInstance<TimeService>();

        System.Threading.Thread.Sleep(20);

        try
        {
            while (this.RenderWindow.IsOpen)
            {
                this.RenderWindow.DispatchEvents();

                time.Update();

                accumulator += time.Current.DeltaTime;
                while (accumulator >= time.FixedDeltaTime)
                {
                    this.UPDATE_FRAME(time.FixedDeltaTime);
                    accumulator -= time.FixedDeltaTime;
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

                    System.Threading.Thread.Sleep(2);
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
        }
        catch (System.Exception ex)
        {
            NLogixFx.Error($"Unhandled exception in main game loop: {ex}", source: "GraphicsEngine");
        }
        finally
        {
            this.RenderWindow.Dispose();
            try { MusicManager.Dispose(); } catch { }
        }
    }

    /// <summary>
    /// Closes the game window and disposes of systems.
    /// </summary>
    public void Shutdown()
    {
        RenderWindow.Close();

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
    private void UPDATE_FRAME(System.Single deltaTime)
    {
        this.FrameUpdate?.Invoke(deltaTime);

        InputTimeline.Instance.Update();
        KeyboardManager.Instance.Update();
        MouseManager.Instance.Update(RenderWindow);

        SceneManager.Instance.ProcessSceneChange();
        SceneManager.Instance.ProcessPendingDestroy();
        SceneManager.Instance.ProcessPendingSpawn();
        SceneManager.Instance.UpdateSceneObjects(deltaTime);
    }

    /// <summary>
    /// Draws all visible scene objects, sorted by Z-index.
    /// </summary>
    private void UPDATE_DRAW(RenderTarget target)
    {
        if (_renderCacheDirty)
        {
            _renderObjectCache = [.. SceneManager.Instance.GetAllObjectsOfType<RenderObject>()];
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
    private void HANDLE_FOCUS_CHANGED(System.Boolean focused)
    {
        _isFocused = focused;

        if (!GraphicsConfig.VSync)
        {
            this.RenderWindow.SetFramerateLimit(focused ? _foregroundFps : _backgroundFps);
        }
    }

    #endregion Private Methods
}