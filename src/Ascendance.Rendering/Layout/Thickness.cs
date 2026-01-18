// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Layout;

/// <summary>
/// Represents thickness values for each side of a rectangle.
/// Commonly used for 9-slice scaling, margins, or padding.
/// </summary>
public readonly struct Thickness(System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom)
{
    /// <summary>
    /// Gets the thickness of the left side.
    /// </summary>
    public System.Int32 Left { get; } = left;

    /// <summary>
    /// Gets the thickness of the top side.
    /// </summary>
    public System.Int32 Top { get; } = top;

    /// <summary>
    /// Gets the thickness of the right side.
    /// </summary>
    public System.Int32 Right { get; } = right;

    /// <summary>
    /// Gets the thickness of the bottom side.
    /// </summary>
    public System.Int32 Bottom { get; } = bottom;

    /// <summary>
    /// Initializes a new <see cref="Thickness"/> with the same value for all sides.
    /// </summary>
    /// <param name="uniform">
    /// The thickness applied uniformly to left, top, right, and bottom.
    /// </param>
    public Thickness(System.Int32 uniform)
        : this(System.Math.Max(0, uniform), System.Math.Max(0, uniform),
               System.Math.Max(0, uniform), System.Math.Max(0, uniform))
    {
    }
}
