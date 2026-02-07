// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Security.AntiCheat.Models;

/// <summary>
/// Represents the result of a cheat detection scan.
/// </summary>
public sealed class CheatDetectionResult
{
    /// <summary>
    /// Gets or sets whether a cheat was detected.
    /// </summary>
    public System.Boolean IsDetected { get; set; }

    /// <summary>
    /// Gets or sets the detection method that found the cheat.
    /// </summary>
    public System.String DetectionMethod { get; set; }

    /// <summary>
    /// Gets or sets the platform where detection occurred.
    /// </summary>
    public System.String Platform { get; set; }

    /// <summary>
    /// Gets or sets additional details about the detection.
    /// </summary>
    public System.String Details { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of detection.
    /// </summary>
    public System.DateTimeOffset Timestamp { get; set; } = System.DateTimeOffset.UtcNow;
}