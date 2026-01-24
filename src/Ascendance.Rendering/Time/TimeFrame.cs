// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Time;

/// <summary>
/// Represents a snapshot of timing information for the current frame.
/// </summary>
public sealed class TimeFrame
{
    /// <summary>
    /// Gets the elapsed time (in seconds) since the last update.
    /// </summary>
    public System.Single DeltaTime { get; internal set; }

    /// <summary>
    /// Gets the total elapsed time since the engine started.
    /// </summary>
    public System.Single TotalTime { get; internal set; }

    /// <summary>
    /// Gets the fixed time step used for deterministic updates.
    /// </summary>
    public System.Single FixedDeltaTime { get; internal set; }
}