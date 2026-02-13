// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.AntiCheat.Models;
using System.Linq;

namespace Ascendance.Security.AntiCheat.Platform;

/// <summary>
/// Windows-specific anti-cheat detection implementation.
/// </summary>
public sealed partial class WindowsAntiCheatDetector : IAntiCheatDetector
{
    #region Constants

    private const System.Boolean SET_LAST_ERROR = true;
    private const System.String DLL_KERNEL32 = "kernel32.dll";
    private const System.String ENTRYPOINT_IS_DEBUGGER_PRESENT = "IsDebuggerPresent";
    private const System.String ENTRYPOINT_CHECK_REMOTE_DEBUGGER = "CheckRemoteDebuggerPresent";

    #endregion Constants

    #region Fields

    private static readonly System.String[] CheatToolNames =
    [
        "cheatengine",
        "cheatengine-x86_64",
        "cheatengine-i386"
    ];

    #endregion Fields

    #region Invoke Declarations

    /// <summary>
    /// Determines whether the specified process is being debugged.
    /// </summary>
    /// <param name="hProcess">A handle to the process.</param>
    /// <param name="isDebuggerPresent">
    /// A pointer to a variable that the function sets to TRUE if the specified process is being debugged, or FALSE otherwise.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_KERNEL32,
        SetLastError = SET_LAST_ERROR,
        EntryPoint = ENTRYPOINT_CHECK_REMOTE_DEBUGGER)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(hProcess))]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial System.Boolean CHECK_REMOTE_DEBUGGER_PRESENT(
        System.IntPtr hProcess,
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    ref System.Boolean isDebuggerPresent);

    /// <summary>
    /// Determines whether the calling process is being debugged by a user-mode debugger.
    /// </summary>
    /// <returns>
    /// If the current process is running in the context of a debugger, the return value is nonzero.
    /// If the current process is not running in the context of a debugger, the return value is zero.
    /// </returns>
    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_KERNEL32,
        SetLastError = SET_LAST_ERROR,
        EntryPoint = ENTRYPOINT_IS_DEBUGGER_PRESENT)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial System.Boolean IS_DEBUGGER_PRESENT();

    #endregion Invoke Declarations

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
                    System.String name = process.ProcessName.ToLowerInvariant();

                    if (CheatToolNames.Any(name.Contains))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Windows] Detected cheat tool: {process.ProcessName}");
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Windows] Error scanning processes: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public System.Boolean IsDebuggerAttached()
    {
        // Method 1: Managed debugger
        if (System.Diagnostics.Debugger.IsAttached)
        {
            return true;
        }

        // Method 2: Native debugger
        if (IS_DEBUGGER_PRESENT())
        {
            return true;
        }

        // Method 3: Remote debugger
        System.Boolean isRemoteDebugger = false;
        CHECK_REMOTE_DEBUGGER_PRESENT(System.Diagnostics.Process.GetCurrentProcess().Handle, ref isRemoteDebugger);

        return isRemoteDebugger;
    }

    /// <inheritdoc/>
    public CheatDetectionResult PerformDetection()
    {
        CheatDetectionResult result = new();

        if (IsCheatToolRunning())
        {
            result.IsDetected = true;
            result.Platform = "Windows";
            result.DetectionMethod = "Process Scanner (Windows)";
        }
        else if (IsDebuggerAttached())
        {
            result.IsDetected = true;
            result.Platform = "Windows";
            result.DetectionMethod = "Debugger Detection (Windows)";
        }

        return result;
    }
}