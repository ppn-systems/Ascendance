// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Audio;

namespace Ascendance.Rendering.Managers;

/// <summary>
/// Manages music playback, resource caching, and control.
/// </summary>
public static class MusicManager
{
    #region Fields

    private static Music _currentMusic;
    private static readonly System.Collections.Generic.Dictionary<System.String, Music> _musicLibrary = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets whether any music is currently playing.
    /// </summary>
    public static System.Boolean IsPlaying => _currentMusic?.Status == SoundStatus.Playing;

    /// <summary>
    /// Gets whether the current music is paused.
    /// </summary>
    public static System.Boolean IsPaused => _currentMusic?.Status == SoundStatus.Paused;

    /// <summary>
    /// Gets the current music (readonly, may be null).
    /// </summary>
    public static Music CurrentMusic => _currentMusic;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Loads a music file into cache, or throws if not found.
    /// </summary>
    /// <param name="filename">Path to music file (must exist).</param>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public static void Load(System.String filename)
    {
        if (_musicLibrary.ContainsKey(filename))
        {
            return;
        }

        if (!System.IO.File.Exists(filename))
        {
            throw new System.IO.FileNotFoundException($"Music file not found: {filename}");
        }

        _musicLibrary[filename] = new Music(filename);
    }

    /// <summary>
    /// Plays a music file, loading if necessary.
    /// </summary>
    /// <param name="filename">Path to music file.</param>
    /// <param name="loop">True to loop playback.</param>
    /// <exception cref="System.IO.FileNotFoundException">If file not found and cannot be loaded.</exception>
    public static void PlayMusic(System.String filename, System.Boolean loop = true)
    {
        Stop();

        if (!_musicLibrary.TryGetValue(filename, out var music))
        {
            Load(filename); // Try auto-load
            music = _musicLibrary[filename];
        }

        _currentMusic = music;
        _currentMusic.Loop = loop;
        _currentMusic.Play();
    }

    /// <summary>
    /// Pauses the currently playing music (if any).
    /// </summary>
    public static void Pause() => _currentMusic?.Pause();

    /// <summary>
    /// Resumes playback if current music is paused.
    /// </summary>
    public static void Resume()
    {
        if (_currentMusic?.Status == SoundStatus.Paused)
        {
            _currentMusic.Play();
        }
    }

    /// <summary>
    /// Stops and unloads current music.
    /// </summary>
    public static void Stop()
    {
        _currentMusic?.Stop();
        _currentMusic = null;
    }

    /// <summary>
    /// Sets the volume for the current music.
    /// </summary>
    /// <param name="volume">Volume [0..100].</param>
    public static void SetMusicVolume(System.Single volume)
    {
        if (_currentMusic == null)
        {
            return;
        }

        _currentMusic.Volume = System.Single.Clamp(volume, 0.0f, 100.0f);
    }

    /// <summary>
    /// Frees all loaded music tracks from cache.
    /// </summary>
    public static void ClearMusicCache()
    {
        foreach (var music in _musicLibrary.Values)
        {
            music.Dispose();
        }

        _musicLibrary.Clear();
        _currentMusic = null;
    }

    /// <summary>
    /// Fully releases all resources and stops any playback.
    /// </summary>
    public static void Dispose()
    {
        Stop();
        ClearMusicCache();
    }

    #endregion APIs
}