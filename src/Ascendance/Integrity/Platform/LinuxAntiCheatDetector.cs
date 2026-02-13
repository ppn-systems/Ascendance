// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.Integrity.Models;
using System.Linq;

namespace Ascendance.Integrity.Platform;

/// <summary>
/// Linux-specific anti-cheat detection implementation.
/// </summary>
[System.Diagnostics.DebuggerNonUserCode]
public sealed class LinuxAntiCheatDetector : IAntiCheatDetector
{
    private static readonly System.String[] CheatToolNames =
    [
        "scanmem",
        "gameconqueror",
        "gdb",
        "strace",
        "ltrace"
    ];

    /// <inheritdoc/>
    public System.Boolean IsCheatToolRunning()
    {
        try
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    System.String name = process.ProcessName.ToLowerInvariant();

                    if (CheatToolNames.Any(name.Contains))
                    {
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public System.Boolean IsDebuggerAttached()
    {
        try
        {
            // Check /proc/self/status for TracerPid
            System.String statusPath = "/proc/self/status";
            if (System.IO.File.Exists(statusPath))
            {
                System.String[] lines = System.IO.File.ReadAllLines(statusPath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("TracerPid:", System.StringComparison.Ordinal))
                    {
                        System.String[] parts = line.Split(':');
                        if (parts.Length > 1 && System.Int32.TryParse(parts[1].Trim(), out System.Int32 pid))
                        {
                            return pid != 0; // 0 means no debugger
                        }
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public CheatDetectionResult PerformDetection()
    {
        CheatDetectionResult result = new()
        {
            Platform = "Linux",
            Timestamp = System.DateTimeOffset.UtcNow
        };

        if (IsDebuggerAttached())
        {
            result.IsDetected = true;
            result.DetectionMethod = "Debugger Detection (Linux /proc)";
            result.Details = "TracerPid indicates debugger attached";
            return result;
        }

        if (IsCheatToolRunning())
        {
            result.IsDetected = true;
            result.DetectionMethod = "Process Scanner (Linux)";
            result.Details = "Known cheat tool detected";
            return result;
        }

        result.IsDetected = false;
        result.DetectionMethod = "Full Scan (Linux)";
        result.Details = "No threats detected";

        return result;
    }
}