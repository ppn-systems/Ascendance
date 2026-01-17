// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Audio;

namespace Ascendance.Rendering.Loaders;

/// <summary>
/// Sound management class. Handles loading/unloading of unmanaged sound resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the SfxLoader class.
/// </remarks>
/// <param name="assetRoot">Optional root path of the managed asset folder</param>
public sealed class SfxLoader(System.String assetRoot = "") : AssetLoader<SoundBuffer>(AvailableFormats, assetRoot)
{
    /// <summary>
    /// List of supported file endings for this SfxLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> AvailableFormats =
    [
            ".ogg", ".wav", ".flac", ".aiff", ".au", ".raw",
            ".paf", ".svx", ".nist", ".voc", ".ircam", ".w64",
            ".mat4", ".mat5", ".pvf", ".htk", ".sds", ".avr",
            ".sd2", ".caf", ".wve", ".mpc2k", ".rf64"
    ];

    /// <summary>
    /// Loads or retrieves an already loaded instance of a Sound from a Stream Source
    /// </summary>
    /// <param name="name">Name of the Resource</param>
    /// <param name="stream">Readable stream containing the raw data of the sound</param>
    /// <returns>The managed Sound</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public SoundBuffer Load(System.String name, System.IO.Stream stream)
    {
        System.ObjectDisposedException.ThrowIf(Disposed, nameof(SfxLoader));
        System.ArgumentNullException.ThrowIfNull(name);

        if (_Assets.TryGetValue(name, out SoundBuffer value))
        {
            return value;
        }

        if (stream?.CanRead != true)
        {
            throw new System.ArgumentNullException(nameof(stream));
        }

        System.Byte[] data = new System.Byte[stream.Length];
        stream.ReadExactly(data);
        return Load(name, data);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override SoundBuffer CreateInstanceFromRawData(System.Byte[] rawData)
    {
        if (rawData == null || rawData.Length == 0)
        {
            throw new System.ArgumentException("Raw data is null or empty.", nameof(rawData));
        }

        using System.IO.MemoryStream memoryStream = new(rawData, writable: false);
        return new SoundBuffer(memoryStream);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override SoundBuffer CreateInstanceFromPath(System.String path)
        => System.String.IsNullOrWhiteSpace(path) ? throw new System.ArgumentException("Path is null or empty.", nameof(path)) : new SoundBuffer(path);
}
