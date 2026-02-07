// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Nalix.Network.Abstractions;
using Nalix.Network.Listeners.Tcp;

namespace Ascendance.Infrastructure.Listeners;

/// <summary>
/// TCP listener responsible for handling core gameplay traffic.
/// This is the main listener for all game-related communication after authentication.
/// </summary>
public sealed class GameTcpListener : TcpListenerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameTcpListener"/> class.
    /// </summary>
    /// <param name="protocol">The game protocol to handle incoming connections.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public GameTcpListener(IProtocol protocol)
        : base(port: 7777, protocol: protocol)
    {
    }
}