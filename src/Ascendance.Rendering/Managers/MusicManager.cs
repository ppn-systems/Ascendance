// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Audio;

namespace Ascendance.Rendering.Managers;

/// <summary>
/// Manages music playback, resource caching, and control.
/// </summary>
public static class MusicManager
{
    #region Fields

    private static Music _current;
    private static readonly System.Collections.Generic.Dictionary<System.String, Music> _musicCache = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets whether any music is currently playing.
    /// </summary>
    public static System.Boolean IsPlaying => _current?.Status == SoundStatus.Playing;

    /// <summary>
    /// Gets whether the current music is paused.
    /// </summary>
    public static System.Boolean IsPaused => _current?.Status == SoundStatus.Paused;

    /// <summary>
    /// Gets the current music (readonly, may be null).
    /// </summary>
    public static Music Current => _current;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Loads a music file into cache, or throws if not found.
    /// </summary>
    /// <param name="filename">Path to music file (must exist).</param>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public static void Load(System.String filename)
    {
        if (_musicCache.ContainsKey(filename))
        {
            return;
        }

        if (!System.IO.File.Exists(filename))
        {
            throw new System.IO.FileNotFoundException($"Music file not found: {filename}");
        }

        _musicCache[filename] = new Music(filename);
    }

    /// <summary>
    /// Plays a music file, loading if necessary.
    /// </summary>
    /// <param name="filename">Path to music file.</param>
    /// <param name="loop">True to loop playback.</param>
    /// <exception cref="FileNotFoundException">If file not found and cannot be loaded.</exception>
    public static void Play(System.String filename, System.Boolean loop = true)
    {
        Stop();

        if (!_musicCache.TryGetValue(filename, out var music))
        {
            Load(filename); // Try auto-load
            music = _musicCache[filename];
        }

        _current = music;
        _current.Loop = loop;
        _current.Play();
    }

    /// <summary>
    /// Pauses the currently playing music (if any).
    /// </summary>
    public static void Pause() => _current?.Pause();

    /// <summary>
    /// Resumes playback if current music is paused.
    /// </summary>
    public static void Resume()
    {
        if (_current?.Status == SoundStatus.Paused)
        {
            _current.Play();
        }
    }

    /// <summary>
    /// Stops and unloads current music.
    /// </summary>
    public static void Stop()
    {
        _current?.Stop();
        _current = null;
    }

    /// <summary>
    /// Sets the volume for the current music.
    /// </summary>
    /// <param name="volume">Volume [0..100].</param>
    public static void SetVolume(System.Single volume)
    {
        if (_current == null)
        {
            return;
        }

        _current.Volume = System.Single.Clamp(volume, 0.0f, 100.0f);
    }

    /// <summary>
    /// Frees all loaded music tracks from cache.
    /// </summary>
    public static void ClearCache()
    {
        foreach (var music in _musicCache.Values)
        {
            music.Dispose();
        }

        _musicCache.Clear();
        _current = null;
    }

    /// <summary>
    /// Fully releases all resources and stops any playback.
    /// </summary>
    public static void Dispose()
    {
        Stop();
        ClearCache();
    }

    #endregion APIs
}