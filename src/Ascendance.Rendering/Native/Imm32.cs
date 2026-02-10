// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Rendering.Native;

/// <summary>
/// Provides simple helpers to disable and restore the Windows IME (Input Method Editor)
/// for a specific native window. Use this to prevent IME composition UI from appearing
/// while your game window is handling raw keyboard input.
/// </summary>
public static partial class Imm32
{
    #region Constants

    private const System.Boolean SET_LAST_ERROR = false;

    private const System.String DLL_IMM32 = "Imm32.dll";
    private const System.String ENTRYPOINT_IMM32_IMM_ASSOCIATE_CONTEXT = "ImmAssociateContext";

    #endregion Constants

    #region Invoke Declarations

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_IMM32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_IMM32_IMM_ASSOCIATE_CONTEXT,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(IMM_ASSOCIATE_CONTEXT))]
    private static partial System.IntPtr IMM_ASSOCIATE_CONTEXT(System.IntPtr hWnd, System.IntPtr hIMC);

    #endregion Invoke Declarations

    /// <summary>
    /// Disassociates the IME context from the specified window, effectively disabling IME.
    /// Returns the previous IME context (HIMC) so it can be restored later.
    /// </summary>
    /// <param name="hwnd">Native window handle (HWND).</param>
    /// <returns>Previous IME context handle (HIMC) or IntPtr.Zero on failure.</returns>
    public static System.IntPtr DisableIme(System.IntPtr hwnd)
    {
        if (hwnd == System.IntPtr.Zero)
        {
            return System.IntPtr.Zero;
        }

        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
             System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Not supported on non-Windows platforms.
            return System.IntPtr.Zero;
        }

        try
        {
            // Associate a null IME context to disable IME for this window.
            return IMM_ASSOCIATE_CONTEXT(hwnd, System.IntPtr.Zero);
        }
        catch
        {
            return System.IntPtr.Zero;
        }
    }

    /// <summary>
    /// Restores a previous IME context for the specified window.
    /// </summary>
    /// <param name="hwnd">Native window handle (HWND).</param>
    /// <param name="previousContext">HIMC returned by DisableIme.</param>
    public static void RestoreIme(System.IntPtr hwnd, System.IntPtr previousContext)
    {
        if (hwnd == System.IntPtr.Zero)
        {
            return;
        }

        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
             System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return;
        }

        try
        {
            // Re-associate the previously saved IME context.
            IMM_ASSOCIATE_CONTEXT(hwnd, previousContext);
        }
        catch
        {
            // Ignore restore errors.
        }
    }

    /// <summary>
    /// Convenience overload that accepts an SFML RenderWindow and disables IME,
    /// returning the previous IME context.
    /// </summary>
    public static System.IntPtr DisableIme(RenderWindow window)
    {
        if (window is null)
        {
            return System.IntPtr.Zero;
        }

        // SFML.Net exposes the native window handle as SystemHandle.
        System.IntPtr hwnd = window.SystemHandle;
        return DisableIme(hwnd);
    }

    /// <summary>
    /// Restore IME for an SFML RenderWindow using a previously saved HIMC.
    /// </summary>
    public static void RestoreIme(RenderWindow window, System.IntPtr previousContext)
    {
        if (window is null)
        {
            return;
        }

        RestoreIme(window.SystemHandle, previousContext);
    }
}