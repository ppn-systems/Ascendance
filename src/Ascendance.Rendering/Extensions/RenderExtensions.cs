// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Enums;
using SFML.Graphics;

namespace Ascendance.Rendering.Extensions;

/// <inheritdoc/>
internal static class RenderExtensions
{
    /// <inheritdoc/>
    public static System.Int32 ToZIndex(this RenderLayer layer) => (System.Int32)layer;

    /// <inheritdoc/>
    public static void Draw(this RenderTarget target, RenderObject renderObject) => renderObject.Draw(target);
}
