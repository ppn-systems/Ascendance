// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.Abstractions;

/// <summary>
/// Represents an abstraction for drawing overlays for each transition effect.
/// </summary>
public interface ITransitionDrawable
{
    /// <summary>
    /// Gets the <see cref="Drawable"/> instance to be rendered each frame.
    /// </summary>
    /// <returns>
    /// A <see cref="Drawable"/> object representing the current overlay to render.
    /// </returns>
    Drawable GetDrawable();

    /// <summary>
    /// Updates the shape based on the progress value (range [0..1]) and phase.
    /// </summary>
    /// <param name="progress01">A value between 0 and 1 indicating the transition progress.</param>
    /// <param name="closing">If true, the overlay is closing (covering); if false, it is opening (revealing).</param>
    void Update(System.Single progress01, System.Boolean closing);
}