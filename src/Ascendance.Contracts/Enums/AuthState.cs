// Copyright (c) 2026 Ascendance Team. All rights reserved.

namespace Ascendance.Contracts.Enums;

/// <summary>
/// Authentication states for connection lifecycle.
/// </summary>
public enum AuthState : System.Byte
{
    /// <summary>
    /// Initial state, no handshake completed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Handshake completed, awaiting login.
    /// </summary>
    HandshakeComplete = 1,

    /// <summary>
    /// Login completed, ready to disconnect.
    /// </summary>
    LoginComplete = 2
}
