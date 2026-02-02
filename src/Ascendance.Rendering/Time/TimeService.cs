// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using SFML.System;

namespace Ascendance.Rendering.Time;

/// <summary>
/// Provides centralized time measurement and accumulation for the rendering engine.
/// </summary>
/// <remarks>
/// This service is responsible for tracking frame-to-frame timing,
/// total elapsed time, and fixed-step timing used by deterministic systems
/// such as physics or simulations.
/// </remarks>
[System.Diagnostics.DebuggerDisplay("TotalTime={_totalTime}, FixedΔ={FixedDeltaTime}")]
public sealed class TimeService
{
    #region Fields

    private System.Single _totalTime;

    /// <summary>
    /// Internal clock used to measure elapsed real time between frames.
    /// </summary>
    private readonly Clock _clock = InstanceManager.Instance.GetOrCreateInstance<Clock>();

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the timing snapshot for the current frame.
    /// </summary>
    /// <remarks>
    /// The returned instance is updated once per frame during <see cref="Update"/>.
    /// Consumers should treat this data as read-only.
    /// </remarks>
    public TimeFrame Current { get; } = new();

    /// <summary>
    /// Gets the fixed time step, in seconds, used for deterministic update loops.
    /// </summary>
    /// <remarks>
    /// The default value corresponds to a 60 Hz update rate.
    /// </remarks>
    public System.Single FixedDeltaTime { get; } = 1f / 60f;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Updates the timing data for the current frame.
    /// </summary>
    /// <remarks>
    /// This method should be called exactly once per frame.
    /// It clamps excessively large delta times to avoid instability
    /// caused by long frame stalls.
    /// </remarks>
    public void Update()
    {
        System.Single delta = _clock.Restart().AsSeconds();

        // Clamp delta time to avoid extreme spikes (e.g. breakpoint, window drag)
        if (delta > 0.25f)
        {
            delta = 0.25f;
        }

        _totalTime += delta;

        this.Current.DeltaTime = delta;
        this.Current.TotalTime = _totalTime;
        this.Current.FixedDeltaTime = FixedDeltaTime;
    }

    #endregion APIs
}
