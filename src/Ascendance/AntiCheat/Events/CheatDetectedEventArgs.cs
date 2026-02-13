// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.AntiCheat.Events;

/// <summary>
/// Event arguments for cheat detection events.
/// </summary>
public sealed class CheatDetectedEventArgs : System.EventArgs
{
    /// <summary>
    /// Gets the detection method used.
    /// </summary>
    public required System.String DetectionMethod { get; init; }

    /// <summary>
    /// Gets the timestamp of detection.
    /// </summary>
    public required System.DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the total number of detections.
    /// </summary>
    public required System.Int32 TotalDetections { get; init; }

    /// <summary>
    /// Gets the scan number when detection occurred.
    /// </summary>
    public required System.Int32 ScanNumber { get; init; }

    /// <summary>
    /// Gets the platform where detection occurred.
    /// </summary>
    public System.String Platform { get; init; }

    /// <summary>
    /// Gets additional detection details.
    /// </summary>
    public System.String Details { get; init; }
}