// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Theme;
using Ascendance.Shared.Abstractions;
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
    /// <param name="target">The target SFML render window.</param>
    public override void Draw(RenderTarget target)
    {
        if (!_engine.IsDebugMode)
        {
            _customDebugLines.Clear();
            return;
        }

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

    protected override Drawable GetDrawable() => null; // Không sử dụng draw kiểu này

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Compose all debug lines for the overlay.
    /// </summary>
    private System.Collections.Generic.List<System.String> COMPOSE_DEBUG_LINES()
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
            lines.Add($"Mouse: ({Mouse.GetPosition(_engine.RenderWindow).X}, {Mouse.GetPosition(_engine.RenderWindow).Y})");
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

    #region Class

    /// <summary>
    /// Procedural animated spinner used as a loading indicator.
    /// Can be shown independently or embedded as part of composite UI.
    /// </summary>
    /// <remarks>
    /// This spinner is designed for efficient rendering by precomputing segment shapes and alpha multipliers
    /// to avoid unnecessary allocations during each frame.
    /// </remarks>
    private sealed class Spinner : RenderObject, IUpdatable
    {
        #region Constants

        private const System.Int32 SegmentCount = 12;
        private const System.Single SpinnerRadius = 32f;
        private const System.Single SegmentThickness = 7f;
        private const System.Single DegreesToRadians = 0.017453292519943295f;

        #endregion Constants

        #region Fields

        private Vector2f _center;
        private System.Single _currentAngle = 0f;

        // Precomputed values to avoid re-allocating every Draw
        private readonly System.Byte _alpha = 255;
        private readonly System.Single _rotationDegreesPerSecond = 150f;
        private readonly CircleShape[] _segmentShapes = new CircleShape[SegmentCount];
        private readonly System.Single[] _segmentOffsets = new System.Single[SegmentCount];
        private readonly System.Byte[] _segmentAlphaMultipliers = new System.Byte[SegmentCount];

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class at a specific center point.
        /// </summary>
        /// <param name="center">The center point for the spinner.</param>
        public Spinner(Vector2f center)
        {
            _center = center;
            this.PRECOMPUTE_SEGMENTS();
            base.SetZIndex(RenderLayer.Spinner.ToZIndex());
        }

        #endregion Constructor

        #region Main Loop

        /// <inheritdoc />
        public override void Update(System.Single deltaTime)
        {
            _currentAngle += deltaTime * _rotationDegreesPerSecond;
            if (_currentAngle >= 360f)
            {
                _currentAngle -= 360f;
            }
        }

        /// <inheritdoc />
        public override void Draw(RenderTarget target)
        {

            for (System.Int32 i = 0; i < SegmentCount; i++)
            {
                System.Single segAngle = _currentAngle + _segmentOffsets[i];
                System.Single angleRad = segAngle * DegreesToRadians;

                System.Single x = _center.X + (System.MathF.Cos(angleRad) * SpinnerRadius);
                System.Single y = _center.Y + (System.MathF.Sin(angleRad) * SpinnerRadius);

                CircleShape segCircle = _segmentShapes[i];

                segCircle.Radius = SegmentThickness / 2f;
                segCircle.Origin = new Vector2f(segCircle.Radius, segCircle.Radius);
                segCircle.Position = new Vector2f(x, y);

                System.Byte finalAlpha = (System.Byte)(_alpha * _segmentAlphaMultipliers[i] / 255);
                segCircle.FillColor = new Color(Themes.SpinnerForegroundColor.R, Themes.SpinnerForegroundColor.G, Themes.SpinnerForegroundColor.B, finalAlpha);

                target.Draw(segCircle);
            }
        }

        /// <inheritdoc />
        protected override Drawable GetDrawable() =>
            throw new System.NotSupportedException("Spinner uses procedural geometry. Call Render() directly.");

        #endregion Main Loop

        #region Private Methods

        /// <summary>
        /// Precomputes static values for segment angle and multipliers to optimize drawing.
        /// </summary>
        private void PRECOMPUTE_SEGMENTS()
        {
            const System.Single anglePerSegment = 360f / SegmentCount;

            for (System.Int32 i = 0; i < SegmentCount; i++)
            {
                // Offset angle for this segment (degrees)
                _segmentOffsets[i] = i * anglePerSegment;

                // trailing tail alpha effect: 0.2f + 0.8f * progress => multiply by 255 (max alpha)
                System.Single progress = (System.Single)i / SegmentCount;
                System.Single alphaMultiplier = 0.2f + (0.8f * progress);
                _segmentAlphaMultipliers[i] = (System.Byte)(alphaMultiplier * 255);

                // Init CircleShape ONCE, just set position/color each draw
                _segmentShapes[i] = new CircleShape(SegmentThickness / 2f)
                {
                    Origin = new Vector2f(SegmentThickness / 2f, SegmentThickness / 2f)
                };
            }
        }

        #endregion Private Methods
    }

    #endregion Class
}