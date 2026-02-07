// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Security.AntiCheat.Abstractions;
using Ascendance.Desktop.Security.AntiCheat.Configuration;
using Ascendance.Desktop.Security.AntiCheat.Events;
using Ascendance.Desktop.Security.AntiCheat.Models;
using Nalix.Common.Concurrency;
using Nalix.Common.Core.Abstractions;
using Nalix.Common.Diagnostics;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using Nalix.Framework.Injection.DI;
using Nalix.Framework.Options;
using Nalix.Framework.Tasks;

namespace Ascendance.Desktop.Security.AntiCheat;

/// <summary>
/// Orchestrates anti-cheat detection using platform-specific detectors.
/// Manages background scanning via <see cref="TaskManager"/>.
/// </summary>
[System.Diagnostics.DebuggerNonUserCode]
[System.Runtime.CompilerServices.SkipLocalsInit]
public sealed class AntiCheatMonitor : SingletonBase<AntiCheatMonitor>, IActivatable
{
    #region Fields

    private static readonly AntiCheatMonitorOptions DetectorOptions =
        ConfigurationManager.Instance.Get<AntiCheatMonitorOptions>();

    private readonly ILogger _logger;
    private readonly IAntiCheatDetector _platformDetector;
    private readonly System.Int32 _scanIntervalMs;
    private readonly System.Boolean _autoShutdown;
    private readonly System.Int32 _exitCode;

    [System.Diagnostics.CodeAnalysis.AllowNull]
    private System.Threading.CancellationTokenSource _cts;

    private IWorkerHandle _workerHandle;
    private volatile System.Boolean _isActive;

    // Detection state
    private volatile System.Boolean _isCheatEngineDetected;
    private System.Int32 _totalDetections;
    private System.DateTimeOffset? _firstDetectionTime;
    private System.DateTimeOffset? _lastDetectionTime;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when a cheat is detected.
    /// </summary>
    public event System.EventHandler<CheatDetectedEventArgs> CheatDetected;

    #endregion Events

    #region Properties

    /// <summary>
    /// Gets whether the detector is currently active.
    /// </summary>
    public System.Boolean IsActive => _isActive;

    /// <summary>
    /// Gets whether a cheat has been detected.
    /// </summary>
    public System.Boolean IsCheatEngineDetected => _isCheatEngineDetected;

    /// <summary>
    /// Gets the total number of detections.
    /// </summary>
    public System.Int32 TotalDetections => System.Threading.Volatile.Read(ref _totalDetections);

    /// <summary>
    /// Gets the timestamp of the first detection.
    /// </summary>
    public System.DateTimeOffset? FirstDetectionTime => _firstDetectionTime;

    /// <summary>
    /// Gets the timestamp of the most recent detection.
    /// </summary>
    public System.DateTimeOffset? LastDetectionTime => _lastDetectionTime;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance using configuration and auto-detected platform detector.
    /// </summary>
    public AntiCheatMonitor()
        : this(AntiCheatDetectorFactory.Create(), null)
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom detector and options.
    /// </summary>
    /// <param name="platformDetector">The platform-specific detector implementation.</param>
    /// <param name="options">Optional configuration. If null, uses <see cref="ConfigurationManager"/>.</param>
    public AntiCheatMonitor(IAntiCheatDetector platformDetector, AntiCheatMonitorOptions options = null)
    {
        System.ArgumentNullException.ThrowIfNull(platformDetector);

        _platformDetector = platformDetector;
        _logger = InstanceManager.Instance.GetExistingInstance<ILogger>();

        AntiCheatMonitorOptions opts = options ?? DetectorOptions;
        opts.Validate();

        _scanIntervalMs = opts.ScanIntervalMs;
        _autoShutdown = opts.AutoShutdownOnDetection;
        _exitCode = opts.ExitCode;

        _logger?.Info($"[AC.{nameof(AntiCheatMonitor)}] init detector={platformDetector.GetType().Name} " +
                      $"interval={_scanIntervalMs}ms autoShutdown={_autoShutdown}");
    }

    #endregion Constructors

    #region IActivatable

    /// <inheritdoc/>
    [System.Diagnostics.DebuggerStepThrough]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Activate(System.Threading.CancellationToken cancellationToken = default)
    {
        if (_cts is { IsCancellationRequested: false })
        {
            _logger?.Warn($"[AC.{nameof(AntiCheatMonitor)}:{nameof(Activate)}] already active");
            return;
        }

        System.Threading.CancellationTokenSource linkedCts = cancellationToken.CanBeCanceled
            ? System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : new System.Threading.CancellationTokenSource();

        _cts = linkedCts;
        System.Threading.CancellationToken linkedToken = linkedCts.Token;

        _workerHandle = InstanceManager.Instance.GetOrCreateInstance<TaskManager>().ScheduleWorker(
            name: $"CheatEngineDetector_{_scanIntervalMs}ms",
            group: "AntiCheat",
            work: async (ctx, ct) => await DetectionLoopAsync(ctx, ct).ConfigureAwait(false),
            options: new WorkerOptions
            {
                CancellationToken = linkedToken,
                Tag = nameof(AntiCheatMonitor),
                RetainFor = System.TimeSpan.Zero,
                GroupConcurrencyLimit = 1,
                TryAcquireSlotImmediately = true
            }
        );

        _isActive = true;

        _logger?.Info($"[AC.{nameof(AntiCheatMonitor)}:{nameof(Activate)}] activated workerId={_workerHandle.Id}");
    }

    /// <inheritdoc/>
    [System.Diagnostics.StackTraceHidden]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Deactivate(System.Threading.CancellationToken cancellationToken = default)
    {
        var cts = System.Threading.Interlocked.Exchange(ref _cts, null);
        if (cts is null)
        {
            return;
        }

        _isActive = false;

        try { cts.Cancel(); }
        catch (System.Exception ex)
        {
            _logger?.Warn($"[AC.{nameof(AntiCheatMonitor)}:{nameof(Deactivate)}] cancel-error msg={ex.Message}");
        }

        if (_workerHandle is not null)
        {
            var tm = InstanceManager.Instance.GetExistingInstance<TaskManager>();
            tm?.CancelWorker(_workerHandle.Id);
            _workerHandle = null;
        }

        try { cts.Dispose(); }
        catch (System.Exception ex)
        {
            _logger?.Warn($"[AC.{nameof(AntiCheatMonitor)}:{nameof(Deactivate)}] dispose-error msg={ex.Message}");
        }

        _logger?.Info($"[AC.{nameof(AntiCheatMonitor)}:{nameof(Deactivate)}] deactivated totalDetections={TotalDetections}");
    }

    #endregion IActivatable

    #region Detection Loop

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private async System.Threading.Tasks.Task DetectionLoopAsync(
        IWorkerContext ctx,
        System.Threading.CancellationToken ct)
    {
        _logger?.Debug($"[AC.{nameof(AntiCheatMonitor)}:Internal] loop-start workerId={ctx.Id}");

        try
        {
            using System.Threading.PeriodicTimer timer = new(
                System.TimeSpan.FromMilliseconds(_scanIntervalMs));

            System.Int32 scanCount = 0;

            while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                scanCount++;

                try
                {
                    // Delegate to platform-specific detector
                    CheatDetectionResult result = _platformDetector.PerformDetection();

                    if (result.IsDetected)
                    {
                        HandleDetection(scanCount, result);

                        if (_autoShutdown)
                        {
                            _logger?.Fatal($"[AC.{nameof(AntiCheatMonitor)}:Internal] auto-shutdown triggered!");
                            System.Environment.Exit(_exitCode);
                        }
                    }
                }
                catch (System.OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (System.Exception ex)
                {
                    _logger?.Error($"[AC.{nameof(AntiCheatMonitor)}:Internal] scan-error scan={scanCount}", ex);
                }

                ctx.Beat();
            }
        }
        catch (System.OperationCanceledException)
        {
            _logger?.Debug($"[AC.{nameof(AntiCheatMonitor)}:Internal] loop-cancelled");
        }
        catch (System.Exception ex)
        {
            _logger?.Error($"[AC.{nameof(AntiCheatMonitor)}:Internal] loop-error", ex);
        }
        finally
        {
            _logger?.Debug($"[AC.{nameof(AntiCheatMonitor)}:Internal] loop-end");
        }
    }

    #endregion Detection Loop

    #region Event Handling

    private void HandleDetection(System.Int32 scanNumber, CheatDetectionResult result)
    {
        _isCheatEngineDetected = true;
        System.Int32 count = System.Threading.Interlocked.Increment(ref _totalDetections);

        System.DateTimeOffset now = result.Timestamp;

        _firstDetectionTime ??= now;
        _lastDetectionTime = now;

        _logger?.Warn($"[AC.{nameof(AntiCheatMonitor)}:Internal] 🚨 CHEAT DETECTED! " +
                     $"scan={scanNumber} total={count} method={result.DetectionMethod} platform={result.Platform}");

        try
        {
            CheatDetected?.Invoke(this, new CheatDetectedEventArgs
            {
                DetectionMethod = result.DetectionMethod ?? "Unknown",
                Timestamp = now,
                TotalDetections = count,
                ScanNumber = scanNumber,
                Platform = result.Platform,
                Details = result.Details
            });
        }
        catch (System.Exception ex)
        {
            _logger?.Error($"[AC.{nameof(AntiCheatMonitor)}:Internal] event-handler-error", ex);
        }
    }

    #endregion Event Handling

    #region IDisposable

    /// <summary>
    /// Disposes the detector and stops monitoring.
    /// </summary>
    public new void Dispose() => Deactivate();

    #endregion IDisposable
}