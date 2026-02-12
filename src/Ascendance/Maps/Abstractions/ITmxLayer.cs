// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Collections;

namespace Ascendance.Maps.Abstractions;

/// <summary>
/// Common contract for layers in a Tiled map (tile layers, object groups,
/// image layers and group layers).
/// </summary>
public interface ITmxLayer : ITmxElement
{
    /// <summary>
    /// Horizontal offset in pixels for the layer.
    /// Nullable to indicate the attribute was not present in the TMX.
    /// </summary>
    System.Double? OffsetX { get; }

    /// <summary>
    /// Vertical offset in pixels for the layer.
    /// Nullable to indicate the attribute was not present in the TMX.
    /// </summary>
    System.Double? OffsetY { get; }

    /// <summary>
    /// Layer opacity (0.0 - 1.0). Implementations should default to 1.0 when the attribute is absent.
    /// </summary>
    System.Double Opacity { get; }


    /// <summary>
    /// Whether the layer is visible. Implementations should default to true when the attribute is absent.
    /// </summary>
    System.Boolean Visible { get; }


    /// <summary>
    /// Custom properties attached to this layer.
    /// </summary>
    PropertyDict Properties { get; }
}