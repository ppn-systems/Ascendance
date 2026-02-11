// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Animation;
using Ascendance.Shared.Enums;
using Ascendance.Shared.Protocol;
using SFML.System;

namespace Ascendance.Characters;

/// <summary>
/// Client-side representation of a remote player entity.
/// Buffers server snapshots (PlayerSnapshotPacket), performs interpolation/extrapolation,
/// and synchronizes animation state with the local animator or PlayerController.
/// </summary>
public sealed class CharactersRemote : System.IDisposable
{
    #region Constants

    private const System.Int32 MAX_EXTRAPOLATE_MS = 200;
    private const System.Int32 DEFAULT_BUFFER_CAPACITY = 64;
    private const System.Int32 DEFAULT_INTERPOLATION_DELAY_MS = 120;

    #endregion Constants

    #region Types

    /// <summary>
    /// Internal snapshot derived from network packet for interpolation logic.
    /// </summary>
    private struct Snapshot
    {
        public System.Int64 ServerTimestampMs;
        public Vector2f Position;
        public Vector2f Velocity;
        public PlayerState State;
        public Direction2D Direction;
        public System.Int32 AnimationFrameIndex;
        public System.Single? AnimationProgress;
    }

    #endregion Types

    #region Fields

    private readonly Animator _animator;
    private readonly System.Threading.Lock _lock;
    private readonly System.Int32 _bufferCapacity;
    private readonly CharacterController _animationController; // optional, may be null
    private readonly System.Collections.Generic.LinkedList<Snapshot> _buffer;

    private System.Boolean _disposed;
    private System.Int32 _interpolationDelayMs;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Create a RemotePlayer.
    /// </summary>
    /// <param name="animator">Animator that renders this remote player's sprite (required).</param>
    /// <param name="animationController">Optional PlayerController to map state+direction to animation frames.</param>
    /// <param name="interpolationDelayMs">Interpolation delay in milliseconds (default 120ms).</param>
    /// <param name="bufferCapacity">Snapshot buffer capacity (default 64).</param>
    /// <exception cref="System.ArgumentNullException">Thrown when animator is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public CharactersRemote(
        Animator animator,
        CharacterController animationController = null,
        System.Int32 interpolationDelayMs = DEFAULT_INTERPOLATION_DELAY_MS,
        System.Int32 bufferCapacity = DEFAULT_BUFFER_CAPACITY)
    {
        _lock = new();
        _buffer = new();

        _animator = animator ?? throw new System.ArgumentNullException(nameof(animator));
        _animationController = animationController;
        _interpolationDelayMs = System.Math.Max(0, interpolationDelayMs);
        _bufferCapacity = System.Math.Max(8, bufferCapacity);
    }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Clears the snapshot buffer.
    /// </summary>
    public void ClearBuffer()
    {
        lock (_lock)
        {
            _buffer.Clear();
        }
    }

    /// <summary>
    /// Update the remote player's transform and animation for this frame.
    /// localNowMs must be synced to server time (serverTime = localTime + offset).
    /// </summary>
    /// <param name="localNowMs">Local time in milliseconds aligned to server clock.</param>
    public void Update(System.Int64 localNowMs)
    {
        Snapshot? s0 = null;
        Snapshot? s1 = null;

        lock (_lock)
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            System.Int64 renderTime = localNowMs - _interpolationDelayMs;

            var node = _buffer.First;
            while (node != null)
            {
                var snap = node.Value;
                if (snap.ServerTimestampMs <= renderTime)
                {
                    s0 = snap;
                }

                if (snap.ServerTimestampMs >= renderTime)
                {
                    s1 = snap;
                    break;
                }

                node = node.Next;
            }

            if (!s0.HasValue && _buffer.First != null)
            {
                s0 = _buffer.First.Value;
            }

            if (!s1.HasValue && _buffer.Last != null)
            {
                s1 = _buffer.Last.Value;
            }
        } // release lock

        if (!s0.HasValue)
        {
            return; // nothing to render
        }

        Vector2f finalPos;

        if (s1.HasValue && s0.Value.ServerTimestampMs != s1.Value.ServerTimestampMs)
        {
            // Interpolate between s0 and s1
            System.Int64 renderTime = localNowMs - _interpolationDelayMs;
            System.Single t = (renderTime - s0.Value.ServerTimestampMs) / (System.Single)(s1.Value.ServerTimestampMs - s0.Value.ServerTimestampMs);
            t = CLAMPF(t, 0f, 1f);
            finalPos = LERP(s0.Value.Position, s1.Value.Position, t);

            // Prefer later state for animation
            APPLY_ANIMATION_STATE(s1.Value);
        }
        else
        {
            // Single snapshot available -> snap or extrapolate
            var latest = s0.Value;
            System.Int64 renderTime = localNowMs - _interpolationDelayMs;
            System.Int32 dtMs = (System.Int32)(renderTime - latest.ServerTimestampMs);

            if (dtMs <= 0)
            {
                finalPos = latest.Position;
            }
            else
            {
                System.Int32 cap = System.Math.Min(dtMs, MAX_EXTRAPOLATE_MS);
                finalPos = new Vector2f(
                    latest.Position.X + (latest.Velocity.X * (cap / 1000f)),
                    latest.Position.Y + (latest.Velocity.Y * (cap / 1000f))
                );
            }

            APPLY_ANIMATION_STATE(latest);
        }

        // Apply computed position to animator's sprite
        _animator.Sprite.Position = finalPos;
    }

    /// <summary>
    /// Push a PlayerSnapshotPacket received from network into the buffer.
    /// This method will convert the packet to internal Snapshot and insert it ordered by timestamp.
    /// </summary>
    public void PushPacket(PlayerSnapshotPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        Snapshot snap = CONVERT_PACKET_TO_SNAPSHOT(packet);

        lock (_lock)
        {
            if (_buffer.Count == 0)
            {
                _buffer.AddLast(snap);
            }
            else
            {
                var node = _buffer.Last;
                while (node != null && node.Value.ServerTimestampMs > snap.ServerTimestampMs)
                {
                    node = node.Previous;
                }

                if (node == null)
                {
                    _buffer.AddFirst(snap);
                }
                else
                {
                    _buffer.AddAfter(node, snap);
                }
            }

            // Trim buffer to capacity (remove oldest)
            while (_buffer.Count > _bufferCapacity)
            {
                _buffer.RemoveFirst();
            }
        }
    }

    /// <summary>
    /// Set interpolation delay (how far behind "now" the client renders).
    /// Typical range: 80 - 200 ms depending on RTT/jitter.
    /// </summary>
    public void SetInterpolationDelay(System.Int32 ms) => _interpolationDelayMs = System.Math.Max(0, ms);

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Convert a PlayerSnapshotPacket to internal Snapshot struct.
    /// </summary>
    private static Snapshot CONVERT_PACKET_TO_SNAPSHOT(PlayerSnapshotPacket p)
    {
        // Note: ServerTimestampMs in packet is UInt64. Convert to signed long safely.
        System.Int64 ts = checked((System.Int64)p.ServerTimestampMs);

        var snap = new Snapshot
        {
            ServerTimestampMs = ts,
            Position = new Vector2f(p.PositionX, p.PositionY),
            Velocity = new Vector2f(p.VelocityX, p.VelocityY),
            State = p.State,
            Direction = p.Direction,
            AnimationFrameIndex = p.AnimationFrameIndex,
            AnimationProgress = (p.AnimationFrameIndex >= 0) ? null : (p.AnimationProgress > 0f ? p.AnimationProgress : null)
        };

        return snap;
    }

    /// <summary>
    /// Apply animation state to animator.
    /// Priority:
    /// 1) If AnimationFrameIndex provided -> GoToFrame
    /// 2) If AnimationProgress provided -> compute frame from progress
    /// 3) If PlayerController provided -> call UpdateAnimation(state, direction)
    /// 4) Otherwise leave animator unchanged
    /// </summary>
    private void APPLY_ANIMATION_STATE(Snapshot snapshot)
    {
        if (_animator == null)
        {
            return;
        }

        if (snapshot.AnimationFrameIndex >= 0 && snapshot.AnimationFrameIndex < _animator.FrameCount)
        {
            _animator.GoToFrame(snapshot.AnimationFrameIndex);
            return;
        }

        if (snapshot.AnimationProgress.HasValue && _animator.FrameCount > 0)
        {
            System.Single prog = CLAMPF(snapshot.AnimationProgress.Value, 0f, 1f);
            System.Int32 frame = (System.Int32)System.MathF.Floor(prog * (_animator.FrameCount - 1));
            frame = System.Math.Clamp(frame, 0, _animator.FrameCount - 1);
            _animator.GoToFrame(frame);
            return;
        }

        if (_animationController != null)
        {
            _animationController.UpdateAnimation(snapshot.State, snapshot.Direction);
            return;
        }

        // Otherwise: do nothing (preserve current animator state)
    }

    private static Vector2f LERP(Vector2f a, Vector2f b, System.Single t)
    {
        return new Vector2f(
            a.X + ((b.X - a.X) * t),
            a.Y + ((b.Y - a.Y) * t)
        );
    }

    private static System.Single CLAMPF(System.Single value, System.Single min, System.Single max) => value < min ? min : value > max ? max : value;

    #endregion Private Methods

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        lock (_lock)
        {
            _buffer.Clear();
        }
        // Note: Animator and PlayerController are owned externally and should not be disposed here.
    }

    #endregion IDisposable
}