// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Shared.Enums;

/// <summary>
/// Specifies the tile layer rendering priority and type classification.
/// </summary>
/// <remarks>
/// Used to organize layers by their purpose and control rendering order.
/// Each layer type has an associated render order value for automatic z-sorting.
/// </remarks>
public enum TileLayerType : System.Int16
{
    /// <summary>
    /// Background layer rendered behind all other layers.
    /// </summary>
    /// <remarks>
    /// Render order: -100. Used for sky, distant scenery, or parallax backgrounds.
    /// </remarks>
    Background = -100,

    /// <summary>
    /// Collision layer (usually invisible) used for physics calculations.
    /// </summary>
    /// <remarks>
    /// Render order: -1. Typically not rendered in release builds.
    /// </remarks>
    Collision = -1,

    /// <summary>
    /// Ground/terrain layer where the player walks.
    /// </summary>
    /// <remarks>
    /// Render order: 0. Primary gameplay surface layer.
    /// </remarks>
    Ground = 0,

    /// <summary>
    /// Decoration layer for non-interactive visual elements.
    /// </summary>
    /// <remarks>
    /// Render order: 50. Grass, flowers, small details that don't affect gameplay.
    /// </remarks>
    Decoration = 50,

    /// <summary>
    /// Foreground layer rendered in front of entities.
    /// </summary>
    /// <remarks>
    /// Render order: 100. Tree tops, overhangs, or elements that obscure the player.
    /// </remarks>
    Foreground = 100,

    /// <summary>
    /// UI or overlay layer rendered on top of everything.
    /// </summary>
    /// <remarks>
    /// Render order: 200. Used for fog, lighting effects, or debug visualization.
    /// </remarks>
    Overlay = 200
}