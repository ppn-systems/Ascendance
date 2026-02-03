// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.UI.Indicators;

/// <summary>
/// Renders debug information overlay on the game window with high efficiency and extensibility. Use AddCustomLine to append custom debug info.
/// </summary>
public class DebugOverlay : RenderObject
{
    #region Fields

    private readonly Font _font;
    private readonly Clock _fpsClock;
    private readonly GraphicsEngine _engine;
    private readonly System.UInt32 _fontSize;
    private readonly System.Single _lineSpacing;
    private readonly System.Collections.Generic.List<System.String> _customDebugLines = [];
    private readonly System.Collections.Generic.List<Text> _textObjects = [];

    private System.Int32 _frameCount;
    private System.Single _currentFps;

    #endregion Fields

    #region Properties

    public Vector2f Origin { get; set; } = new Vector2f(10, 10);
    public Color FontColor { get; set; } = Color.White;
    public Color OutlineColor { get; set; } = Color.Black;
    public System.Single OutlineThickness { get; set; } = 1f;
    public System.Boolean ShowObjectsInfo { get; set; } = true;
    public System.Boolean ShowMemory { get; set; } = true;
    public System.Boolean ShowInput { get; set; } = true;
    public System.Boolean ShowScene { get; set; } = true;
    public System.Boolean ShowVSync { get; set; } = true;
    public System.Boolean ShowWindow { get; set; } = true;


    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugOverlay"/> class.
    /// </summary>
    /// <param name="font">Debug font, must not be null.</param>
    /// <param name="fontSize">Font size, default 16.</param>
    /// <param name="lineSpacing">Line spacing, default 20.</param>
    public DebugOverlay(Font font, System.UInt32 fontSize = 16, System.Single lineSpacing = 20f)
    {
        _fpsClock = new();
        _fontSize = fontSize;
        _lineSpacing = lineSpacing;
        _engine = GraphicsEngine.Instance;
        _font = font ?? throw new System.ArgumentNullException(nameof(font));

        base.SetZIndex(RenderLayer.Highest.ToZIndex());
    }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Call this method to add custom debug text for this frame.
    /// </summary>
    public void AddCustomLine(System.String line)
    {
        if (!System.String.IsNullOrEmpty(line))
        {
            _customDebugLines.Add(line);
        }
    }

    /// <summary>
    /// Renders the debug overlay if debug mode is enabled.
    /// </summary>
    /// <param name="window">The target SFML render window.</param>
    public void Draw(RenderWindow window)
    {
        if (!_engine.IsDebugMode)
        {
            _customDebugLines.Clear();
            return;
        }

        UPDATE_FPS();
        var lines = COMPOSE_DEBUG_LINES(window);

        // Ensure object pool matches needed lines
        while (_textObjects.Count < lines.Count)
        {
            _textObjects.Add(new Text(System.String.Empty, _font, _fontSize));
        }

        System.Single y = Origin.Y;
        for (System.Int32 i = 0; i < lines.Count; ++i)
        {
            var text = _textObjects[i];
            text.DisplayedString = lines[i];
            text.Position = new Vector2f(Origin.X, y);
            text.FillColor = FontColor;
            text.OutlineColor = OutlineColor;
            text.OutlineThickness = OutlineThickness;
            window.Draw(text);
            y += _lineSpacing;
        }
        _customDebugLines.Clear();
    }

    protected override Drawable GetDrawable() => null; // Không sử dụng draw kiểu này

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Compose all debug lines for the overlay.
    /// </summary>
    private System.Collections.Generic.List<System.String> COMPOSE_DEBUG_LINES(RenderWindow window)
    {
        var lines = new System.Collections.Generic.List<System.String>
        {
            $"FPS: {_currentFps:F1}"
        };

        if (ShowScene)
        {
            lines.Add($"Scene: {SceneManager.Instance.GetActiveSceneName()}");
        }

        if (ShowObjectsInfo)
        {
            lines.Add($"Objects: {GraphicsEngine.Instance.ActiveObjectCount}");
        }

        if (ShowMemory)
        {
            lines.Add($"Managed Memory: {System.GC.GetTotalMemory(false) / 1048576} MB");
        }

        if (ShowVSync)
        {
            lines.Add($"VSync: {(GraphicsEngine.GraphicsConfig.VSync ? "On" : "Off")}");
        }

        if (ShowWindow)
        {
            lines.Add($"Window: {GraphicsEngine.ScreenSize.X} x {GraphicsEngine.ScreenSize.Y}");
        }

        lines.Add($"Debug: {_engine.IsDebugMode}");

        if (ShowInput)
        {
            lines.Add($"Mouse: ({Mouse.GetPosition(window).X}, {Mouse.GetPosition(window).Y})");
        }

        // Show custom lines at the end
        lines.AddRange(_customDebugLines);

        return lines;
    }

    /// <summary>
    /// Calculates and updates the FPS counter.
    /// </summary>
    private void UPDATE_FPS()
    {
        _frameCount++;
        System.Single elapsed = _fpsClock.ElapsedTime.AsSeconds();
        if (elapsed >= 1f)
        {
            _currentFps = _frameCount / elapsed;
            _fpsClock.Restart();
            _frameCount = 0;
        }
    }

    #endregion Private Methods
}