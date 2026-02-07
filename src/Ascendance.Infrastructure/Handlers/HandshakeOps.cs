// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Ascendance.Infrastructure.Extensions;
using Ascendance.Shared.Enums;
using Nalix.Common.Connection;
using Nalix.Common.Core.Enums;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Messaging.Packets.Attributes;
using Nalix.Common.Messaging.Protocols;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging.Controls;
using Nalix.Shared.Security.Asymmetric;
using Nalix.Shared.Security.Hashing;

namespace Ascendance.Infrastructure.Handlers;

/// <summary>
/// Manages the secure handshake process to establish an encrypted connection with the client.
/// Uses X25519 key exchange algorithm and SHA3256 hashing to ensure connection security and integrity.
/// This class is responsible for initiating the handshake, generating key pairs, and computing the shared encryption key.
/// </summary>
[PacketController]
public sealed class HandshakeOps
{
    static HandshakeOps()
    {
        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .SetMaxCapacity<Handshake>(1024);

        _ = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Prealloc<Handshake>(64);
    }

    /// <summary>
    /// Initiates the secure handshake process with the client.
    /// Receives a packet containing the client's X25519 public key (32 bytes), generates the server's X25519 key pair,
    /// computes the shared encryption key, and sends the server's public key back to the client.
    /// This method validates the packet format to ensure safety and efficiency.
    /// </summary>
    /// <param name="p">Packet containing the client's X25519 public key, requires binary format and length of 32 bytes.</param>
    /// <param name="connection">Connection information of the client requesting secure handshake.</param>
    /// <returns>Packet containing the server's public key or error message if the process fails.</returns>
    [PacketEncryption(false)]
    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.HANDSHAKE)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static async System.Threading.Tasks.Task Handshake(
        IPacket p,
        IConnection connection)
    {
        if (p is not Handshake packet)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.UNSUPPORTED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Error("Invalid packet type. Expected HandshakePacket from {0}", connection.RemoteEndPoint);

            return;
        }

        // Defensive programming - validate null payload
        if (packet.Data is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MISSING_REQUIRED_FIELD,
                ProtocolAdvice.FIX_AND_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Error("Null payload in handshake packet from {0}", connection.RemoteEndPoint);

            return;
        }

        // Validate public key length, must be exactly 32 bytes according to X25519 standard
        if (packet.Data.Length != 32)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY).ConfigureAwait(false);

            NLogix.Host.Instance.Debug(
                "Invalid public key length [Length={0}] from {1}", packet.Data.Length, connection.RemoteEndPoint);

            return;
        }

        // Create response packet containing server's public key
        System.Byte[] payload = [];
        Handshake response = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                     .Get<Handshake>();

        try
        {
            // Generate X25519 key pair (private and public keys) for server
            X25519.X25519KeyPair keyPair = X25519.GenerateKeyPair();

            // Compute shared secret from server's private key and client's public key
            System.Byte[] secret = X25519.Agreement(keyPair.PrivateKey, packet.Data);

            // Hash shared secret using Keccak256 to create secure encryption key
            connection.Secret = Keccak256.HashData(secret);

            // Security: Clear sensitive data from memory
            System.Array.Clear(keyPair.PrivateKey, 0, keyPair.PrivateKey.Length);
            System.Array.Clear(secret, 0, secret.Length);

            // Upgrade client's permission level to Guest
            connection.Level = PermissionLevel.GUEST;

            // ✅ Set auth state to indicate handshake is complete
            connection.SetAuthState(AuthState.HandshakeComplete);

            response.Initialize(keyPair.PublicKey);
            response.OpCode = (System.UInt16)OpCommand.HANDSHAKE;

            payload = response.Serialize();
        }
        catch (System.Exception ex)
        {
            // Error handling according to security best practices
            NLogix.Host.Instance.Error("HANDSHAKE failed for {0}: {1}", connection.RemoteEndPoint, ex.Message);

            // Reset connection state on error
            connection.Secret = null;
            connection.Level = PermissionLevel.NONE;
            connection.SetAuthState(AuthState.None);

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.BACKOFF_RETRY,
                flags: ControlFlags.IS_TRANSIENT).ConfigureAwait(false);
        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Return(response);
        }

        if (payload is { Length: > 0 })
        {
            // If send fails, rollback state to avoid "half-upgraded" connection
            System.Boolean sent = await connection.TCP.SendAsync(payload).ConfigureAwait(false);
            if (!sent)
            {
                connection.Secret = null;
                connection.Level = PermissionLevel.NONE;
                connection.SetAuthState(AuthState.None);
                NLogix.Host.Instance.Warn("HANDSHAKE send failed; rolled back state for {0}", connection.RemoteEndPoint);
                return;
            }

            NLogix.Host.Instance.Info("HANDSHAKE completed for {0} - awaiting LOGIN", connection.RemoteEndPoint);
        }
    }
}