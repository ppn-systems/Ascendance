// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes.Backgrounds;
using SFML.Graphics;

namespace Ascendance.Desktop.Scenes.Main.View;

/// <summary>
/// View component for rendering a parallax background, commonly used in the main menu.
/// </summary>
public sealed class ParallaxLayerView : RenderObject
{
    #region Classes

    /// <summary>
    /// Preset configuration for a parallax background, including multiple background layers.
    /// </summary>
    public sealed class ParallaxPreset
    {
        /// <summary>
        /// Preset variant (for example, theme or style identifier).
        /// </summary>
        public System.Int32 Variant { get; init; }

        /// <summary>
        /// The collection of background layer presets for this parallax configuration.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<Layer> Layers { get; init; } = [];

        /// <summary>
        /// Represents a single layer preset in a parallax background.
        /// </summary>
        public sealed class Layer
        {
            /// <summary>
            /// The speed multiplier for this layer's parallax motion.
            /// </summary>
            public System.Single Speed { get; init; }

            /// <summary>
            /// Whether this layer should repeat horizontally.
            /// </summary>
            public System.Boolean Repeat { get; init; }

            /// <summary>
            /// The file path to the texture for this layer.
            /// </summary>
            public System.String TexturePath { get; init; } = System.String.Empty;
        }
    }

    #endregion Classes

    #region Fields

    private readonly ParallaxBackground _c;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new <see cref="ParallaxLayerView"/> with the specified parallax configuration and z-index.
    /// </summary>
    /// <param name="preset">Parallax background preset.</param>
    /// <param name="zIndex">Z-index for sorting the render order.</param>
    public ParallaxLayerView(ParallaxPreset preset, System.Int32 zIndex)
    {
        _c = new ParallaxBackground(GraphicsEngine.ScreenSize);
        for (System.Int32 i = 0; i < preset.Layers.Count; i++)
        {
            _c.AddBackgroundLayer(AssetManager.Instance
              .LoadTexture(preset.Layers[i].TexturePath), preset.Layers[i].Speed, preset.Layers[i].Repeat);
        }

        base.SetZIndex(zIndex);
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Updates the parallax background layers.
    /// </summary>
    /// <param name="dt">Delta time (thời gian đã trôi qua kể từ lần cập nhật trước).</param>
    public override void Update(System.Single dt) => _c.Update(dt);

    /// <summary>
    /// Renders the parallax background to the specified target if visible.
    /// </summary>
    /// <param name="target">The render target (đối tượng cùng loại với màn hình cần vẽ).</param>
    public override void Draw(RenderTarget target)
    {
        if (!base.IsVisible)
        {
            return;
        }

        _c.Draw(target);
    }

    /// <summary>
    /// Throw NotSupportedException. Use <see cref="Draw(RenderTarget)"/> for custom drawing logic.
    /// </summary>
    /// <returns>Never returns.</returns>
    protected override Drawable GetDrawable() => throw new System.NotSupportedException("Use Render() instead of GetDrawable().");

    #endregion Public Methods
}