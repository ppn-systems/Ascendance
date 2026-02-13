// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Integrity.Detection;

/// <summary>
/// Information about a cheat detection event.
/// </summary>
public sealed class CheatDetectionInfo
{
    /// <summary>
    /// Optional: Detailed process information.
    /// </summary>
    public ProcessInfo ProcessInfo { get; init; }

    /// <summary>
    /// When the detection occurred.
    /// </summary>
    public System.DateTime Timestamp { get; init; }

    /// <summary>
    /// The platform where detection occurred.
    /// </summary>
    public System.String Platform { get; init; } = System.String.Empty;

    /// <summary>
    /// The detection method used.
    /// </summary>
    public System.String DetectionMethod { get; init; } = System.String.Empty;

    /// <inheritdoc/>
    public override System.String ToString()
    {
        System.String info = $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Platform}] Cheat detected via {DetectionMethod}";

        if (ProcessInfo != null)
        {
            info += $"\n  Process: {ProcessInfo.ProcessName} (PID: {ProcessInfo.ProcessId})";
            if (!System.String.IsNullOrEmpty(ProcessInfo.WindowTitle))
            {
                info += $"\n  Window: {ProcessInfo.WindowTitle}";
            }
            info += $"\n  Started: {ProcessInfo.StartTime:yyyy-MM-dd HH:mm:ss}";
        }

        return info;
    }
}
