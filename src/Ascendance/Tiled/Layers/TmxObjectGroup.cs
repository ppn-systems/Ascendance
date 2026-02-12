// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Tiled.Abstractions;
using Ascendance.Tiled.Collections;
using Ascendance.Tiled.Core;
using Ascendance.Tiled.Enums;
using Ascendance.Tiled.Objects;

namespace Ascendance.Tiled.Layers;

/// <summary>
/// Represents an object group (&lt;objectgroup&gt;) layer in a Tiled map.
/// </summary>
public class TmxObjectGroup : ITmxLayer
{
    #region Properties

    /// <summary>
    /// The layer name.
    /// </summary>
    public System.String Name { get; }

    // TODO: Legacy (Tiled Java) attributes (x, y, width, height)

    /// <summary>
    /// Layer color (often used to tint objects in editors).
    /// </summary>
    public TmxColor Color { get; }

    /// <summary>
    /// Draw order for objects in this group.
    /// </summary>
    public DrawOrderType DrawOrder { get; }

    /// <summary>
    /// Layer opacity (0.0 - 1.0). Implementations should default to 1.0 when attribute is absent.
    /// </summary>
    public System.Double Opacity { get; }

    /// <summary>
    /// Whether the layer is visible. Defaults to true when attribute is absent.
    /// </summary>
    public System.Boolean Visible { get; }

    /// <summary>
    /// Horizontal offset in pixels. Nullable to indicate the attribute was not present.
    /// </summary>
    public System.Double? OffsetX { get; }

    /// <summary>
    /// Vertical offset in pixels. Nullable to indicate the attribute was not present.
    /// </summary>
    public System.Double? OffsetY { get; }

    /// <summary>
    /// Objects contained in this group (document order).
    /// </summary>
    public TmxList<TmxObject> Objects { get; }

    /// <summary>
    /// Custom properties attached to this group.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Fields

    // Static lookup to avoid reallocations on each parse.
    private static readonly System.Collections.Generic.Dictionary<System.String, DrawOrderType> s_drawOrderDict = new()
    {
        ["unknown"] = DrawOrderType.UnknownOrder,
        ["topdown"] = DrawOrderType.TopDown,
        ["index"] = DrawOrderType.IndexOrder
    };

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Parses an &lt;objectgroup&gt; element into a <see cref="TmxObjectGroup"/>.
    /// </summary>
    /// <param name="xObjectGroup">The &lt;objectgroup&gt; element to parse.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xObjectGroup"/> is null.</exception>
    public TmxObjectGroup(System.Xml.Linq.XElement xObjectGroup)
    {
        System.ArgumentNullException.ThrowIfNull(xObjectGroup);

        this.Color = new TmxColor(xObjectGroup.Attribute("color"));
        this.Name = (System.String)xObjectGroup.Attribute("name") ?? System.String.Empty;

        this.Opacity = (System.Double?)xObjectGroup.Attribute("opacity") ?? 1.0;
        this.Visible = (System.Boolean?)xObjectGroup.Attribute("visible") ?? true;

        // Keep nullable semantics so callers can detect "attribute absent".
        this.OffsetX = (System.Double?)xObjectGroup.Attribute("offsetx");
        this.OffsetY = (System.Double?)xObjectGroup.Attribute("offsety");

        System.String drawOrderValue = (System.String)xObjectGroup.Attribute("draworder");
        this.DrawOrder = drawOrderValue != null && s_drawOrderDict.TryGetValue(drawOrderValue, out var dt) ? dt : DrawOrderType.UnknownOrder;

        this.Objects = [];
        foreach (var e in xObjectGroup.Elements("object"))
        {
            this.Objects.Add(new TmxObject(e));
        }

        this.Properties = new PropertyDict(xObjectGroup.Element("properties"));
    }

    #endregion Constructor
}