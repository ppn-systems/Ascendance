// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.Maps.Collections;

namespace Ascendance.Maps.Core;

/// <summary>
/// Represents an image layer in a Tiled map. Image layers contain a single image and
/// optional layer-level properties such as visibility, opacity and offsets.
/// </summary>
public class TmxImageLayer : ITmxLayer
{
    #region Properties

    /// <summary>
    /// The layer name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Legacy width (in pixels) provided by some Tiled exports; may be null when absent.
    /// </summary>
    public System.Int32? Width { get; }

    /// <summary>
    /// Legacy height (in pixels) provided by some Tiled exports; may be null when absent.
    /// </summary>
    public System.Int32? Height { get; }

    /// <summary>
    /// Whether the image layer is visible.
    /// </summary>
    public System.Boolean Visible { get; }

    /// <summary>
    /// The layer opacity (0.0 - 1.0).
    /// </summary>
    public System.Double Opacity { get; }

    /// <summary>
    /// Horizontal offset in pixels. Defaults to 0.0 when not specified.
    /// </summary>
    public System.Double? OffsetX { get; }

    /// <summary>
    /// Vertical offset in pixels. Defaults to 0.0 when not specified.
    /// </summary>
    public System.Double? OffsetY { get; }

    /// <summary>
    /// The image referenced by this layer.
    /// </summary>
    public TmxImage Image { get; }

    /// <summary>
    /// Custom properties attached to this layer.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="TmxImageLayer"/> from the given XML element.
    /// </summary>
    /// <param name="xImageLayer">XML element representing the &lt;imagelayer&gt;.</param>
    /// <param name="tmxDir">Directory of the TMX file (used when resolving image paths). Optional.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xImageLayer"/> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when the required &lt;image&gt; element is missing.</exception>
    public TmxImageLayer(System.Xml.Linq.XElement xImageLayer, System.String tmxDir = "")
    {
        System.ArgumentNullException.ThrowIfNull(xImageLayer);

        this.Width = (System.Int32?)xImageLayer.Attribute("width");
        this.Height = (System.Int32?)xImageLayer.Attribute("height");
        this.Opacity = (System.Double?)xImageLayer.Attribute("opacity") ?? 1.0;
        this.OffsetX = (System.Double?)xImageLayer.Attribute("offsetx") ?? 0.0;
        this.OffsetY = (System.Double?)xImageLayer.Attribute("offsety") ?? 0.0;
        this.Visible = (System.Boolean?)xImageLayer.Attribute("visible") ?? true;
        this.Name = (System.String)xImageLayer.Attribute("name") ?? System.String.Empty;

        System.Xml.Linq.XElement xImage = xImageLayer.Element("image")
            ?? throw new System.ArgumentException("The <imagelayer> element must contain an <image> child.", nameof(xImageLayer));

        this.Image = new TmxImage(xImage, tmxDir);

        this.Properties = new PropertyDict(xImageLayer.Element("properties"));
    }

    #endregion Constructor
}