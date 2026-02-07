// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Nalix.Network.Abstractions;
using Nalix.Network.Listeners.Tcp;

namespace Ascendance.Infrastructure.Listeners;

/// <summary>
/// TCP listener responsible for handling authentication and login requests.
/// This listener operates on a separate port from the main game listener
/// to isolate authentication traffic and improve security.
/// </summary>
public sealed class AuthTcpListener : TcpListenerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthTcpListener"/> class.
    /// </summary>
    /// <param name="protocol">The authentication protocol to handle incoming connections.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public AuthTcpListener(IProtocol protocol)
        : base(port: 7776, protocol: protocol)
    {
    }
}