// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Audio;

namespace Ascendance.Rendering.Managers;

/// <summary>
/// Represents a single Sound Effect
/// </summary>
public class SoundManager : System.IDisposable
{
    private SoundBuffer _Buffer;
    private Sound[] _Sounds;

    /// <summary>
    /// Gets the name of this <see cref="SoundManager"/>.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Determines whether this <see cref="SoundManager"/> has been disposed.
    /// </summary>
    public System.Boolean Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundManager" /> class.
    /// </summary>
    /// <param name="name">The sounds name</param>
    /// <param name="soundBuffer">Sound buffer containing the audio data to play with the sound instance</param>
    /// <param name="parallelSounds">The maximum number of parallel playing sounds.</param>
    public SoundManager(System.String name, SoundBuffer soundBuffer, System.Int32 parallelSounds)
    {
        if (System.String.IsNullOrWhiteSpace(name))
        {
            throw new System.ArgumentException($"Invalid {nameof(name)}:{name}");
        }

        Name = name;
        _Buffer = soundBuffer ?? throw new System.ArgumentNullException(nameof(soundBuffer));
        _Sounds = new Sound[System.Math.Clamp(parallelSounds, 1, 25)];
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SoundManager" /> class.
    /// </summary>
    ~SoundManager()
    {
        Dispose(false);
    }

    /// <summary>
    /// Retrieves a sound when available. The amount of sounds per frame is limited.
    /// </summary>
    /// <returns>The sound instance or null when too many instances of the same sound are already active</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public Sound GetSound()
    {
        System.ObjectDisposedException.ThrowIf(Disposed, Name);

        for (System.Int32 i = 0; i < _Sounds.Length; i++)
        {
            var sound = _Sounds[i];
            if (sound == null)
            {
                _Sounds[i] = sound = new Sound(_Buffer);
            }

            if (sound.Status != SoundStatus.Stopped)
            {
                continue;
            }

            return sound;
        }
        return null; // when all sounds are busy none shall be added
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual void Dispose(System.Boolean disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                for (System.Int32 i = 0; i < _Sounds.Length; i++)
                {
                    _Sounds[i]?.Dispose();
                    _Sounds[i] = null;
                }
            }
            _Sounds = null;
            _Buffer = null;
            Disposed = true;
        }
    }
}
