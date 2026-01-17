// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Loaders;
using Nalix.Common.Environment;
using Nalix.Framework.Injection.DI;
using SFML.Audio;
using SFML.Graphics;

namespace Ascendance.Rendering.Managers;

/// <summary>
/// Centralized manager that handles textures, fonts, and sound effects loading/unloading.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssetManager"/> class.
/// </remarks>
/// <param name="root">The root directory for assets.</param>
public sealed class AssetManager(System.String root = null!) : SingletonBase<AssetManager>, System.IDisposable
{
    /// <summary>
    /// Gets the sound effects loader instance.
    /// </summary>
    public SfxLoader SfxLoader { get; } = new SfxLoader(root ?? Directories.BaseAssetsDirectory);

    /// <summary>
    /// Gets the font loader instance.
    /// </summary>
    public FontLoader FontLoader { get; } = new FontLoader(root ?? Directories.BaseAssetsDirectory);

    /// <summary>
    /// Gets the texture loader instance.
    /// </summary>
    public TextureLoader TextureLoader { get; } = new TextureLoader(root ?? Directories.BaseAssetsDirectory);

    /// <summary>
    /// Load a texture by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="data">The binary data of the texture (optional).</param>
    /// <returns>ScreenSize <see cref="Texture"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Texture LoadTexture(System.String name, System.Byte[] data = null) => TextureLoader.Load(name, data);

    /// <summary>
    /// Load a font by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the font.</param>
    /// <param name="data">The binary data of the font (optional).</param>
    /// <returns>ScreenSize <see cref="Font"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Font LoadFont(System.String name, System.Byte[] data = null) => FontLoader.Load(name, data);

    /// <summary>
    /// Load a sound buffer by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the sound buffer.</param>
    /// <param name="data">The binary data of the sound buffer (optional).</param>
    /// <returns>ScreenSize <see cref="SoundBuffer"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public SoundBuffer LoadSound(System.String name, System.Byte[] data = null) => SfxLoader.Load(name, data);

    /// <summary>
    /// Load a sound buffer by name (from stream).
    /// </summary>
    /// <param name="name">The name of the sound buffer.</param>
    /// <param name="stream">The stream containing the sound buffer data.</param>
    /// <returns>ScreenSize <see cref="SoundBuffer"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public SoundBuffer LoadSound(System.String name, System.IO.Stream stream) => SfxLoader.Load(name, stream);

    /// <summary>
    /// Release all loaded assets.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public new void Dispose()
    {
        SfxLoader.Dispose();
        FontLoader.Dispose();
        TextureLoader.Dispose();
    }
}