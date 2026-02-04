// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Sandbox.Native;

/// <summary>
/// Provides native helpers for controlling the Windows console window.
/// </summary>
[System.Security.SecuritySafeCritical]
[System.Diagnostics.DebuggerNonUserCode]
[System.Runtime.InteropServices.BestFitMapping(false)]
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal static partial class Kernel32
{
    #region Constants

    private const System.Int32 SW_HIDE = 0;
    private const System.Int32 SW_SHOW = 5;

    private const System.Boolean SET_LAST_ERROR = false;
    private const System.String DLL_USER32 = "user32.dll";
    private const System.String DLL_KERNEL32 = "kernel32.dll";

    private const System.String ENTRYPOINT_USER32_SHOW_WINDOW = "ShowWindow";
    private const System.String ENTRYPOINT_USER32_IS_WINDOW_VISIBLE = "IsWindowVisible";
    private const System.String ENTRYPOINT_KERNEL32_GET_CONSOLE_WINDOW = "GetConsoleWindow";

    #endregion Constants

    #region Invoke Declarations

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_KERNEL32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_KERNEL32_GET_CONSOLE_WINDOW,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(GET_CONSOLE_WINDOW))]
    private static partial System.IntPtr GET_CONSOLE_WINDOW();

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_USER32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_USER32_SHOW_WINDOW,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial System.Boolean SHOW_WINDOW(System.IntPtr hWnd, System.Int32 nCmdShow);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_USER32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_USER32_IS_WINDOW_VISIBLE,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial System.Boolean IS_WINDOW_VISIBLE(System.IntPtr hWnd);

    #endregion Invoke Declarations

    #region APIs

    /// <summary>
    /// Hides the current process console window if it exists.
    /// </summary>
    [System.Runtime.CompilerServices.SkipLocalsInit]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Hide()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();

        if (handle != System.IntPtr.Zero)
        {
            Kernel32.SHOW_WINDOW(handle, SW_HIDE);
        }
    }

    /// <summary>
    /// Shows the current process console window if it exists.
    /// </summary>
    [System.Runtime.CompilerServices.SkipLocalsInit]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Show()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();

        if (handle != System.IntPtr.Zero)
        {
            Kernel32.SHOW_WINDOW(handle, SW_SHOW);
        }
    }

    /// <summary>
    /// Checks whether the current process console window is visible.
    /// </summary>
    public static System.Boolean IsConsoleVisible()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();
        return handle != System.IntPtr.Zero && Kernel32.IS_WINDOW_VISIBLE(handle);
    }

    #endregion APIs
}