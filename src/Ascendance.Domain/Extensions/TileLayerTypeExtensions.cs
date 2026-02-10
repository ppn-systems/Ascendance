// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Domain.Enums;

namespace Ascendance.Domain.Extensions;

/// <summary>
/// Extension methods for <see cref="TileLayerType"/>.
/// </summary>
public static class TileLayerTypeExtensions
{
    /// <summary>
    /// Gets the default render order for the layer type.
    /// </summary>
    /// <param name="layerType">The layer type.</param>
    /// <returns>The render order value (lower values render first).</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Int32 GetRenderOrder(this TileLayerType layerType) => (System.Int32)layerType;

    /// <summary>
    /// Determines whether this layer type should be rendered by default.
    /// </summary>
    /// <param name="layerType">The layer type.</param>
    /// <returns><c>true</c> if the layer should be visible; otherwise, <c>false</c>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean IsVisibleByDefault(this TileLayerType layerType) => layerType != TileLayerType.Collision;
}