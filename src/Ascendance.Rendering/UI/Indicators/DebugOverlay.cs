// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
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
/// Renders debug information overlay on the game window with high efficiency and extensibility.
/// </summary>
public class DebugOverlay : RenderObject
{
    #region Fields

    private readonly Font _font;
    private readonly Clock _fpsClock;
    private readonly GraphicsEngine _engine;
    private readonly System.UInt32 _fontSize;
    private readonly System.Single _lineSpacing;
    private readonly System.Collections.Generic.List<Text> _textObjects = [];
    private readonly System.Collections.Generic.List<System.String> _customDebugLines = [];

    private System.Int32 _frameCount;
    private System.Single _currentFps;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the top-left origin for displaying the debug overlay.
    /// </summary>
    public Vector2f Origin { get; set; } = new Vector2f(10, 10);

    /// <summary>
    /// Gets or sets the color used for debug overlay text.
    /// </summary>
    public Color FontColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the outline color for debug overlay text.
    /// </summary>
    public Color OutlineColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets the thickness of outline applied to debug text.
    /// </summary>
    public System.Single OutlineThickness { get; set; } = 1f;

    /// <summary>
    /// Gets or sets a value indicating whether to show object count information in overlay.
    /// </summary>
    public System.Boolean ShowObjectsInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show managed memory usage in overlay.
    /// </summary>
    public System.Boolean ShowMemory { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show input (mouse position) in overlay.
    /// </summary>
    public System.Boolean ShowInput { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show scene information in overlay.
    /// </summary>
    public System.Boolean ShowScene { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show VSync status in overlay.
    /// </summary>
    public System.Boolean ShowVSync { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show window resolution in overlay.
    /// </summary>
    public System.Boolean ShowWindow { get; set; } = true;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugOverlay"/> class.
    /// </summary>
    /// <param name="font">Debug font, must not be null.</param>
    /// <param name="fontSize">Font size for debug text. Default is 16.</param>
    /// <param name="lineSpacing">Vertical spacing between lines of debug text. Default is 20.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="font"/> is null.</exception>
    public DebugOverlay(Font font = null, System.UInt32 fontSize = 15, System.Single lineSpacing = 20f)
    {
        _fpsClock = new();
        _fontSize = fontSize;
        _lineSpacing = lineSpacing;
        _engine = GraphicsEngine.Instance;
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();

        base.SetZIndex(RenderLayer.Highest.ToZIndex());
    }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Renders the debug overlay if debug mode is enabled.
    /// </summary>
    /// <param name="target">The target SFML render window for drawing.</param>
    public override void Draw(RenderTarget target)
    {
        UPDATE_FPS();
        System.Collections.Generic.List<System.String> lines = COMPOSE_DEBUG_LINES();

        // Ensure object pool matches needed lines
        while (_textObjects.Count < lines.Count)
        {
            _textObjects.Add(new Text(System.String.Empty, _font, _fontSize));
        }

        System.Single y = Origin.Y;

        for (System.Int32 i = 0; i < lines.Count; ++i)
        {
            Text text = _textObjects[i];

            text.FillColor = FontColor;
            text.DisplayedString = lines[i];
            text.OutlineColor = OutlineColor;
            text.OutlineThickness = OutlineThickness;
            text.Position = new Vector2f(Origin.X, y);

            target.Draw(text);
            y += _lineSpacing;
        }

        _customDebugLines.Clear();
    }

    /// <inheritdoc/>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() => null; // Không sử dụng kiểu draw này

    #endregion APIs

    #region Private Methods

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

    private static System.String GET_MEMORY()
    {
        System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();

        System.Single usedMb = process.WorkingSet64 / 1048576f;
        System.Single allocatedMb = System.GC.GetTotalMemory(false) / 1048576f;

        return $"Mem: {usedMb:F0}MB   Allocated: {allocatedMb:F0}MB";
    }

    private System.Collections.Generic.List<System.String> COMPOSE_DEBUG_LINES()
    {
        System.Collections.Generic.List<System.String> lines =
        [
            // 1. Tổng quan window & mode
            $"Window Size: {GraphicsEngine.ScreenSize.X} x {GraphicsEngine.ScreenSize.Y}",
            $"Debug Mode: {_engine.IsDebugMode}",
            $"Vertical Sync: {(GraphicsEngine.GraphicsConfig.VSync ? "On" : "Off")}",

            // 2. Thông tin FPS, Frame, Time
            $"FPS: {_currentFps:00.0}   Frame: {_frameCount:00}   Time: {System.DateTime.Now:HH:mm:ss}",

            // 3. Perf (Logic, Render, Memory)
            GET_MEMORY(),
            $"Logic: {_engine.LogicUpdateMilliseconds:00.00} ms   Render: {_engine.RenderFrameMilliseconds:00.00} ms",

            // 4. Input
            $"Mouse Position: ({Mouse.GetPosition(_engine.RenderWindow).X}, {Mouse.GetPosition(_engine.RenderWindow).Y})",

            // 5. Scene/Objects
            $"Scene: {SceneManager.Instance.GetActiveSceneName()}   Rendered Objects: {_engine.ActiveObjectCount}",
        ];

        lines.AddRange(_customDebugLines);

        return lines;
    }

    #endregion Private Methods
}