// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Integrity.Models;

namespace Ascendance.Abstractions;

/// <summary>
/// Platform-agnostic interface for anti-cheat detection.
/// </summary>
public interface IAntiCheatDetector
{
    /// <summary>
    /// Checks if Cheat Engine or similar tools are running.
    /// </summary>
    System.Boolean IsCheatToolRunning();

    /// <summary>
    /// Checks if a debugger is attached to the process.
    /// </summary>
    System.Boolean IsDebuggerAttached();

    /// <summary>
    /// Gets detailed information about detected cheat tools.
    /// </summary>
    CheatDetectionResult PerformDetection();
}