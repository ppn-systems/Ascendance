// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using SFML.System;

namespace Ascendance.Rendering.Time;

/// <summary>
/// Central service responsible for time measurement and accumulation.
/// </summary>
public sealed class TimeService
{
    #region Fields

    private System.Single _totalTime;
    private readonly Clock _clock = InstanceManager.Instance.GetOrCreateInstance<Clock>();

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the current time frame data.
    /// </summary>
    public TimeFrame Current { get; } = new();

    /// <summary>
    /// Gets the fixed delta time (in seconds) for each update step. Default is 1/60 seconds.
    /// </summary>
    public System.Single FixedDeltaTime { get; } = 1f / 60f;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Updates the timing data for the current frame.
    /// </summary>
    public void Update()
    {
        System.Single delta = _clock.Restart().AsSeconds();

        if (delta > 0.25f)
        {
            delta = 0.25f;
        }

        _totalTime += delta;

        Current.DeltaTime = delta;
        Current.TotalTime = _totalTime;
        Current.FixedDeltaTime = FixedDeltaTime;
    }

    #endregion APIs
}