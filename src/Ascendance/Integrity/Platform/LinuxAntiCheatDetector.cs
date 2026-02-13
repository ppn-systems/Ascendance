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

            foreach (System.Diagnostics.Process process in processes)
            {
                try
                {
                    if (CheatToolNames.Any(process.ProcessName.ToLowerInvariant().Contains))
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
                foreach (System.String line in System.IO.File.ReadAllLines(statusPath))
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

        if (this.IsDebuggerAttached())
        {
            result.IsDetected = true;
            result.Details = "TracerPid indicates debugger attached";
            result.DetectionMethod = "Debugger Detection (Linux /proc)";
        }
        else if (this.IsCheatToolRunning())
        {
            result.IsDetected = true;
            result.Details = "Known cheat tool detected";
            result.DetectionMethod = "Process Scanner (Linux)";
        }
        else
        {
            result.IsDetected = false;
            result.Details = "No threats detected";
            result.DetectionMethod = "Full Scan (Linux)";
        }

        return result;
    }
}