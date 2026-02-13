// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.AntiCheat;
using Nalix.Framework.Configuration.Binding;

namespace Ascendance.Configuration;

/// <summary>
/// Configuration options for <see cref="AntiCheatMonitor"/>.
/// </summary>
public sealed class AntiCheatMonitorOptions : ConfigurationLoader
{
    /// <summary>
    /// Gets or sets the scan interval in milliseconds. Default is 3000ms (3 seconds).
    /// </summary>
    public System.Int32 ScanIntervalMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets whether to automatically shutdown when cheat is detected.
    /// </summary>
    public System.Boolean AutoShutdownOnDetection { get; set; } = true;

    /// <summary>
    /// Gets or sets the exit code for auto-shutdown. Default is -1.
    /// </summary>
    public System.Int32 ExitCode { get; set; } = -1;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if scan interval is invalid.</exception>
    public void Validate()
    {
        if (ScanIntervalMs <= 0)
        {
            throw new System.ArgumentOutOfRangeException(
                nameof(ScanIntervalMs),
                "Scan interval must be greater than zero");
        }
    }
}