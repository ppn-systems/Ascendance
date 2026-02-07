// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Nalix.Common.Connection;
using Nalix.Common.Diagnostics;
using Nalix.Common.Infrastructure.Connection;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Network.Protocols;

namespace Ascendance.Infrastructure.Protocols;

/// <summary>
/// Protocol for handling authentication-related messages.
/// Processes login, registration, and token validation requests.
/// </summary>
public sealed class AuthProtocol : Protocol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthProtocol"/> class.
    /// </summary>
    public AuthProtocol()
    {
        // Authentication connections are typically short-lived
        // Close after processing auth request
        this.KeepConnectionOpen = false;
        this.IsAccepting = true;
    }

    /// <summary>
    /// Processes incoming authentication messages.
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
    /// Validates incoming authentication connections.
    /// Can implement IP whitelist, rate limiting, etc.
    /// </summary>
    /// <param name="connection">The connection to validate.</param>
    /// <returns>True if connection is valid, false otherwise.</returns>
    protected override System.Boolean ValidateConnection(IConnection connection)
    {
        // TODO: Implement connection validation
        // Example:
        // 1. Check IP blacklist
        // 2. Rate limiting per IP
        // 3. Geographic restrictions

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
    }
}