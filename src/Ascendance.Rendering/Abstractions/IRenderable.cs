// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.Abstractions;

/// <summary>
/// Represents an interface for renderable objects.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Draws the object on the specified render target.
    /// </summary>
    void Draw(RenderTarget target);
}
