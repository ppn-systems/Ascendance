// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;
using Ascendance.Integrity.Platform;

namespace Ascendance.Integrity;

/// <summary>
/// Factory for creating platform-specific anti-cheat detectors.
/// </summary>
[System.Diagnostics.DebuggerNonUserCode]
internal static class AntiCheatDetectorFactory
{
    /// <summary>
    /// Creates an appropriate <see cref="IAntiCheatDetector"/> for the current platform.
    /// </summary>
    /// <returns>A platform-specific detector instance.</returns>
    /// <exception cref="System.PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static IAntiCheatDetector Create()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return new WindowsAntiCheatDetector();
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Linux))
        {
            return new LinuxAntiCheatDetector();
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.OSX))
        {
            // TODO: Implement MacOSAntiCheatDetector
            throw new System.PlatformNotSupportedException("macOS anti-cheat detection not yet implemented");
        }

        throw new System.PlatformNotSupportedException(
            $"Platform '{System.Runtime.InteropServices.RuntimeInformation.OSDescription}' is not supported for anti-cheat detection");
    }
}