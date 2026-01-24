// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using SFML.System;

namespace Ascendance.Rendering.Time;

/// <summary>
/// Central service responsible for time measurement and accumulation.
/// </summary>
public sealed class TimeService
{
    private System.Single _totalTime;
    private readonly Clock _clock = InstanceManager.Instance.GetOrCreateInstance<Clock>();

    public TimeFrame Current { get; } = new();

    public System.Single FixedDeltaTime { get; } = 1f / 60f;

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
}
