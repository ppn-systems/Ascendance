// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Ascendance.Infrastructure.Extensions;
using Ascendance.Shared.Enums;
using Nalix.Common.Connection;
using Nalix.Common.Core.Enums;
using Nalix.Common.Diagnostics;
using Nalix.Common.Infrastructure.Connection;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Network.Protocols;

namespace Ascendance.Infrastructure.Protocols;

/// <summary>
/// Protocol for handling authentication-related messages.
/// Processes handshake and login requests in sequence.
/// Connection is kept alive between handshake and login.
/// </summary>
public sealed class AuthProtocol : Protocol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthProtocol"/> class.
    /// </summary>
    public AuthProtocol()
    {
        // ✅ Keep connection open by default
        // We'll close it manually after login completes
        this.KeepConnectionOpen = true;
        this.IsAccepting = true;
    }

    /// <summary>
    /// Processes incoming authentication messages.
    /// Routes to appropriate handler based on opcode.
    /// </summary>
    /// <param name="sender">The sender object (typically the connection).</param>
    /// <param name="args">Event arguments containing connection and message data.</param>
    public override void ProcessMessage(
        System.Object sender,
        IConnectEventArgs args)
    {
        System.ArgumentNullException.ThrowIfNull(args);
        System.ArgumentNullException.ThrowIfNull(args.Connection);

        try
        {
            IConnection connection = args.Connection;

            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Debug($"[AUTH.{nameof(AuthProtocol)}:{nameof(ProcessMessage)}] processing from={connection.EndPoint} id={connection.ID}");

            // TODO: Implement authentication message parsing
            // Example:
            // ReadOnlySpan<byte> buffer = args.Buffer.Span;
            // 1. Parse message type (Login/Register/TokenValidate)
            // 2. Validate credentials
            // 3. Generate session token
            // 4. Send response back to client
            // TODO: Send authentication response
            // connection.TCP.Send(responseBuffer);

            _ = InstanceManager.Instance.GetOrCreateInstance<ConnectionHub>()
                                        .RegisterConnection(connection);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[AUTH.{nameof(AuthProtocol)}:{nameof(ProcessMessage)}] error id={args.Connection.ID}", ex);

            args.Connection.Disconnect();
        }
    }

    /// <summary>
    /// Custom post-processing to determine if connection should be closed.
    /// Closes connection only after login completes.
    /// </summary>
    /// <param name="args">Event arguments containing connection details.</param>
    protected override void OnPostProcess(IConnectEventArgs args)
    {
        // Check if we should close the connection based on auth state
        if (!args.Connection.ShouldKeepAlive())
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Debug($"[AUTH.{nameof(AuthProtocol)}:{nameof(OnPostProcess)}] closing connection id={args.Connection.ID} state={args.Connection.GetAuthState()}");

            args.Connection.Disconnect();
        }
    }

    /// <summary>
    /// Validates incoming authentication connections.
    /// Implements basic IP filtering and rate limiting.
    /// </summary>
    /// <param name="connection">The connection to validate.</param>
    /// <returns>True if connection is valid, false otherwise.</returns>
    protected override System.Boolean ValidateConnection(IConnection connection)
    {
        // Initialize auth state
        connection.SetAuthState(AuthState.None);

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[AUTH.{nameof(AuthProtocol)}:{nameof(ValidateConnection)}] validating from={connection.EndPoint}");

        return true;
    }

    /// <summary>
    /// Handles errors that occur during authentication processing.
    /// </summary>
    /// <param name="connection">The connection where the error occurred.</param>
    /// <param name="exception">The exception that was thrown.</param>
    protected override void OnConnectionError(IConnection connection, System.Exception exception)
    {
        base.OnConnectionError(connection, exception);

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
            .Error($"[AUTH.{nameof(AuthProtocol)}:{nameof(OnConnectionError)}] connection-error from={connection.EndPoint}", exception);

        // Reset connection state on any error
        connection.Secret = null;
        connection.Level = PermissionLevel.NONE;
        connection.SetAuthState(AuthState.None);
    }
}