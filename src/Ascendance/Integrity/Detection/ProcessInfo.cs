// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Integrity.Detection;

/// <summary>
/// Information about a detected process.
/// </summary>
public sealed class ProcessInfo
{
    public System.Int32 ProcessId { get; init; }
    public System.DateTime StartTime { get; init; }
    public System.String ProcessName { get; init; } = System.String.Empty;
    public System.String WindowTitle { get; init; } = System.String.Empty;
}
