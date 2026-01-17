// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Layout;

/// <summary>
/// Simple thickness struct to describe 9-slice borders.
/// </summary>
public readonly struct Thickness(System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom)
{
    /// <summary>
    /// Left border thickness.
    /// </summary>
    public System.Int32 Left { get; } = left;

    /// <summary>
    /// Top border thickness.
    /// </summary>
    public System.Int32 Top { get; } = top;

    /// <summary>
    /// Right border thickness.
    /// </summary>
    public System.Int32 Right { get; } = right;

    /// <summary>
    /// Bottom border thickness.
    /// </summary>
    public System.Int32 Bottom { get; } = bottom;

    /// <summary>
    /// Thickness with uniform value for all sides.
    /// </summary>
    public Thickness(System.Int32 uniform) : this(uniform, uniform, uniform, uniform) { }
}

