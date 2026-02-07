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
    #region Constants

    private const System.UInt32 DefaultFontSize = 15;
    private const System.Single DefaultOriginX = 10f;
    private const System.Single DefaultOriginY = 10f;
    private const System.Single FpsUpdateInterval = 1f;
    private const System.Single DefaultLineSpacing = 20f;
    private const System.Byte DefaultBackgroundAlpha = 128;
    private const System.Single BytesToMegabytes = 1048576f;
    private const System.Single DefaultOutlineThickness = 1f;

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly Clock _fpsClock;
    private readonly GraphicsEngine _engine;
    private readonly RectangleShape _background;
    private readonly System.Collections.Generic.List<Text> _textObjects = [];
    private readonly System.Collections.Generic.List<System.String> _customDebugLines = [];
    private readonly System.Collections.Generic.Dictionary<System.String, System.Func<System.String>> _customProviders = [];

    private System.Single _minFps;
    private System.Single _maxFps;
    private System.UInt32 _fontSize;
    private System.Int32 _frameCount;
    private System.Single _avgFpsSum;
    private System.Int32 _avgFpsCount;
    private System.Single _currentFps;
    private System.Single _lineSpacing;
    private System.Single _outlineThickness;
    private DebugOverlayAlignment _alignment;

    #endregion Fields

    #region Enums

    /// <summary>
    /// Defines alignment options for the debug overlay.
    /// </summary>
    public enum DebugOverlayAlignment
    {
        /// <summary>Top-left corner alignment.</summary>
        TopLeft,
        /// <summary>Top-right corner alignment.</summary>
        TopRight,
        /// <summary>Bottom-left corner alignment.</summary>
        BottomLeft,
        /// <summary>Bottom-right corner alignment.</summary>
        BottomRight
    }

    #endregion Enums

    #region Properties

    /// <summary>
    /// Gets or sets the alignment of the debug overlay.
    /// </summary>
    public DebugOverlayAlignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment != value)
            {
                _alignment = value;
                this.UPDATE_ORIGIN_FROM_ALIGNMENT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the top-left origin for displaying the debug overlay.
    /// </summary>
    public Vector2f Origin { get; set; }

    /// <summary>
    /// Gets or sets the font size for debug overlay text.
    /// </summary>
    public System.UInt32 FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                this.REBUILD_TEXT_OBJECTS();
            }
        }
    }

    /// <summary>
    /// Gets or sets the vertical spacing between lines of debug text.
    /// </summary>
    public System.Single LineSpacing
    {
        get => _lineSpacing;
        set => _lineSpacing = System.MathF.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets the color used for debug overlay text.
    /// </summary>
    public Color FontColor { get; set; }

    /// <summary>
    /// Gets or sets the outline color for debug overlay text.
    /// </summary>
    public Color OutlineColor { get; set; }

    /// <summary>
    /// Gets or sets the thickness of outline applied to debug text.
    /// </summary>
    public System.Single OutlineThickness
    {
        get => _outlineThickness;
        set => _outlineThickness = System.MathF.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets whether to show a semi-transparent background behind the overlay.
    /// </summary>
    public System.Boolean ShowBackground { get; set; }

    /// <summary>
    /// Gets or sets the background color (including alpha).
    /// </summary>
    public Color BackgroundColor
    {
        get => _background.FillColor;
        set => _background.FillColor = value;
    }

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

    /// <summary>
    /// Gets or sets a value indicating whether to show FPS information in overlay.
    /// </summary>
    public System.Boolean ShowFps { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show FPS statistics (min/max/avg) in overlay.
    /// </summary>
    public System.Boolean ShowFpsStats { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to show performance metrics in overlay.
    /// </summary>
    public System.Boolean ShowPerformance { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show current timestamp.
    /// </summary>
    public System.Boolean ShowTimestamp { get; set; } = true;

    /// <summary>
    /// Gets the current FPS value.
    /// </summary>
    public System.Single CurrentFps => _currentFps;

    /// <summary>
    /// Gets the minimum FPS recorded since last reset.
    /// </summary>
    public System.Single MinFps => _minFps;

    /// <summary>
    /// Gets the maximum FPS recorded since last reset.
    /// </summary>
    public System.Single MaxFps => _maxFps;

    /// <summary>
    /// Gets the average FPS since last reset.
    /// </summary>
    public System.Single AverageFps => _avgFpsCount > 0 ? _avgFpsSum / _avgFpsCount : 0f;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugOverlay"/> class.
    /// </summary>
    /// <param name="font">Debug font. If null, uses default embedded font.</param>
    /// <param name="fontSize">Font size for debug text. Default is 15.</param>
    /// <param name="lineSpacing">Vertical spacing between lines of debug text. Default is 20.</param>
    public DebugOverlay(
        Font font = null,
        System.UInt32 fontSize = DefaultFontSize,
        System.Single lineSpacing = DefaultLineSpacing)
    {
        _avgFpsSum = 0f;
        _avgFpsCount = 0;
        _fontSize = fontSize;
        _fpsClock = new Clock();
        _lineSpacing = lineSpacing;
        _minFps = System.Single.MaxValue;
        _maxFps = System.Single.MinValue;
        _engine = GraphicsEngine.Instance;
        _alignment = DebugOverlayAlignment.TopLeft;
        _outlineThickness = DefaultOutlineThickness;
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();

        this.ShowBackground = false;
        this.FontColor = Color.White;
        this.OutlineColor = Color.Black;
        this.Origin = new Vector2f(DefaultOriginX, DefaultOriginY);

        _background = new RectangleShape
        {
            FillColor = new Color(0, 0, 0, DefaultBackgroundAlpha)
        };

        base.SetZIndex(RenderLayer.Highest.ToZIndex());
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Adds a custom debug line to be displayed in the current frame.
    /// </summary>
    /// <param name="line">The custom debug information to display.</param>
    public void AddCustomLine(System.String line)
    {
        if (!System.String.IsNullOrEmpty(line))
        {
            _customDebugLines.Add(line);
        }
    }

    /// <summary>
    /// Registers a custom data provider that will be called every frame to generate debug info.
    /// </summary>
    /// <param name="key">Unique identifier for this provider.</param>
    /// <param name="provider">Function that returns a debug string.</param>
    public void RegisterProvider(System.String key, System.Func<System.String> provider)
    {
        if (!System.String.IsNullOrEmpty(key) && provider is not null)
        {
            _customProviders[key] = provider;
        }
    }

    /// <summary>
    /// Unregisters a custom data provider.
    /// </summary>
    /// <param name="key">The identifier of the provider to remove.</param>
    public void UnregisterProvider(System.String key) => _ = _customProviders.Remove(key);

    /// <summary>
    /// Clears all custom debug lines.
    /// </summary>
    public void ClearCustomLines() => _customDebugLines.Clear();

    /// <summary>
    /// Resets FPS statistics (min, max, average).
    /// </summary>
    public void ResetFpsStats()
    {
        _minFps = System.Single.MaxValue;
        _maxFps = System.Single.MinValue;
        _avgFpsSum = 0f;
        _avgFpsCount = 0;
    }

    /// <summary>
    /// Toggles the visibility of the debug overlay.
    /// </summary>
    public void Toggle()
    {
        if (this.IsVisible)
        {
            this.Hide();
        }
        else
        {
            this.Show();
        }
    }

    #endregion Public Methods

    #region Overrides

    /// <summary>
    /// Renders the debug overlay if debug mode is enabled.
    /// </summary>
    /// <param name="target">The target SFML render window for drawing.</param>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.UPDATE_FPS();
        System.Collections.Generic.List<System.String> lines = this.COMPOSE_DEBUG_LINES();

        if (this.ShowBackground)
        {
            this.UPDATE_BACKGROUND_SIZE(lines.Count);
            target.Draw(_background);
        }

        this.ENSURE_TEXT_POOL_SIZE(lines.Count);
        this.RENDER_DEBUG_LINES(target, lines);

        _customDebugLines.Clear();
    }

    /// <inheritdoc/>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() instead.");

    #endregion Overrides

    #region Private Methods - FPS Tracking

    /// <summary>
    /// Updates the FPS counter based on elapsed time.
    /// </summary>
    private void UPDATE_FPS()
    {
        _frameCount++;
        System.Single elapsed = _fpsClock.ElapsedTime.AsSeconds();

        if (elapsed >= FpsUpdateInterval)
        {
            _currentFps = _frameCount / elapsed;

            // Update FPS stats
            if (_currentFps < _minFps)
            {
                _minFps = _currentFps;
            }

            if (_currentFps > _maxFps)
            {
                _maxFps = _currentFps;
            }

            _avgFpsSum += _currentFps;
            _avgFpsCount++;

            _fpsClock.Restart();
            _frameCount = 0;
        }
    }

    #endregion Private Methods - FPS Tracking

    #region Private Methods - Layout

    /// <summary>
    /// Updates the origin position based on the current alignment setting.
    /// </summary>
    private void UPDATE_ORIGIN_FROM_ALIGNMENT()
    {
        Vector2u screenSize = GraphicsEngine.ScreenSize;

        this.Origin = _alignment switch
        {
            DebugOverlayAlignment.TopLeft => new Vector2f(DefaultOriginX, DefaultOriginY),
            DebugOverlayAlignment.TopRight => new Vector2f(screenSize.X - 300f, DefaultOriginY),
            DebugOverlayAlignment.BottomLeft => new Vector2f(DefaultOriginX, screenSize.Y - 300f),
            DebugOverlayAlignment.BottomRight => new Vector2f(screenSize.X - 300f, screenSize.Y - 300f),
            _ => new Vector2f(DefaultOriginX, DefaultOriginY)
        };
    }

    /// <summary>
    /// Updates the background size and position based on text content.
    /// </summary>
    private void UPDATE_BACKGROUND_SIZE(System.Int32 lineCount)
    {
        const System.Single width = 400f;
        System.Single height = (lineCount * _lineSpacing) + 10f;

        _background.Size = new Vector2f(width, height);
        _background.Position = new Vector2f(this.Origin.X - 5f, this.Origin.Y - 5f);
    }

    #endregion Private Methods - Layout

    #region Private Methods - Text Management

    /// <summary>
    /// Ensures the text object pool has enough objects for the required number of lines.
    /// </summary>
    /// <param name="requiredCount">The number of text objects needed.</param>
    private void ENSURE_TEXT_POOL_SIZE(System.Int32 requiredCount)
    {
        while (_textObjects.Count < requiredCount)
        {
            _textObjects.Add(new Text(System.String.Empty, _font, _fontSize));
        }
    }

    /// <summary>
    /// Rebuilds all text objects when font size changes.
    /// </summary>
    private void REBUILD_TEXT_OBJECTS() => _textObjects.Clear();

    /// <summary>
    /// Renders all debug lines to the target.
    /// </summary>
    /// <param name="target">The render target.</param>
    /// <param name="lines">The debug lines to render.</param>
    private void RENDER_DEBUG_LINES(
        RenderTarget target,
        System.Collections.Generic.List<System.String> lines)
    {
        System.Single y = this.Origin.Y;

        for (System.Int32 i = 0; i < lines.Count; i++)
        {
            Text text = _textObjects[i];

            text.DisplayedString = lines[i];
            text.CharacterSize = _fontSize;
            text.FillColor = this.FontColor;
            text.OutlineColor = this.OutlineColor;
            text.OutlineThickness = _outlineThickness;
            text.Position = new Vector2f(this.Origin.X, y);

            target.Draw(text);
            y += _lineSpacing;
        }
    }

    #endregion Private Methods - Text Management

    #region Private Methods - Debug Info Composition

    /// <summary>
    /// Composes all debug lines based on enabled flags.
    /// </summary>
    /// <returns>A list of debug information strings.</returns>
    private System.Collections.Generic.List<System.String> COMPOSE_DEBUG_LINES()
    {
        System.Collections.Generic.List<System.String> lines = [];

        if (this.ShowWindow)
        {
            lines.Add($"Window: {GraphicsEngine.ScreenSize.X} x {GraphicsEngine.ScreenSize.Y}   Debug: {_engine.IsDebugMode}");
        }

        if (this.ShowVSync)
        {
            lines.Add($"VSync: {(GraphicsEngine.GraphicsConfig.VSync ? "On" : "Off")}");
        }

        if (this.ShowFps)
        {
            System.String fpsLine = $"FPS: {_currentFps:00.0}   Frame: {_frameCount:00}";
            if (this.ShowTimestamp)
            {
                fpsLine += $"   Time: {System.DateTime.Now:HH:mm:ss}";
            }
            lines.Add(fpsLine);
        }

        if (this.ShowFpsStats)
        {
            lines.Add($"FPS Stats - Min: {_minFps:00.0}   Max: {_maxFps:00.0}   Avg: {this.AverageFps:00.0}");
        }

        if (this.ShowMemory)
        {
            lines.Add(GET_MEMORY_INFO());
        }

        if (this.ShowPerformance)
        {
            lines.Add($"Logic: {_engine.LogicUpdateMilliseconds:00.00} ms   Render: {_engine.RenderFrameMilliseconds:00.00} ms");
        }

        if (this.ShowInput)
        {
            Vector2i mousePos = Mouse.GetPosition(_engine.RenderWindow);
            lines.Add($"Mouse: ({mousePos.X}, {mousePos.Y})");
        }

        if (this.ShowScene || this.ShowObjectsInfo)
        {
            System.String sceneName = this.ShowScene ? SceneManager.Instance.GetActiveSceneName() : System.String.Empty;
            System.String objectCount = this.ShowObjectsInfo ? $"Objects: {_engine.ActiveObjectCount}" : System.String.Empty;

            if (!System.String.IsNullOrEmpty(sceneName) && !System.String.IsNullOrEmpty(objectCount))
            {
                lines.Add($"Scene: {sceneName}   {objectCount}");
            }
            else if (!System.String.IsNullOrEmpty(sceneName))
            {
                lines.Add($"Scene: {sceneName}");
            }
            else if (!System.String.IsNullOrEmpty(objectCount))
            {
                lines.Add(objectCount);
            }
        }

        // Add custom provider outputs
        foreach (var provider in _customProviders.Values)
        {
            try
            {
                System.String result = provider();
                if (!System.String.IsNullOrEmpty(result))
                {
                    lines.Add(result);
                }
            }
            catch
            {
                // Ignore provider errors
            }
        }

        lines.AddRange(_customDebugLines);

        return lines;
    }

    /// <summary>
    /// Gets formatted memory usage information.
    /// </summary>
    /// <returns>A string containing memory usage details.</returns>
    private static System.String GET_MEMORY_INFO()
    {
        System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();

        System.Single usedMb = process.WorkingSet64 / BytesToMegabytes;
        System.Single allocatedMb = System.GC.GetTotalMemory(false) / BytesToMegabytes;

        return $"Mem: {usedMb:F0}MB   Allocated: {allocatedMb:F0}MB";
    }

    #endregion Private Methods - Debug Info Composition
}