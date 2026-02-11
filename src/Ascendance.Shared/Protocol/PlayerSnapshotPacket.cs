// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Ascendance.Shared.Enums;
using Ascendance.Shared.Extensions;
using Nalix.Common.Core.Attributes;
using Nalix.Common.Infrastructure.Caching;
using Nalix.Common.Messaging.Packets;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;

namespace Ascendance.Shared.Protocol;

/// <summary>
/// Network packet representing a server-authoritative snapshot for a single player entity.
/// </summary>
/// <remarks>
/// - Designed to be compact and suitable for frequent broadcasting from server to clients.
/// - Fields include position, velocity, server timestamp, state, direction and optional animation info.
/// - For optional animation progress, AnimationFrameIndex = -1 means "not provided".
/// </remarks>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.PLAYER_SNAPSHOT)]
public class PlayerSnapshotPacket : FrameBase, IPoolable, IPacketSequenced
{
    // NOTE: Adjust Estimated length if you change fields below.
    [SerializeIgnore]
    public override System.UInt16 Length =>
        PacketConstants.HeaderSize + EstimatedPayloadLength;

    private const System.Int32 EstimatedPayloadLength =
        /* SequenceId */ 4 +
        /* EntityId */ 4 +
        /* ServerTimestampMs */ 8 +
        /* PositionX */ 4 +
        /* PositionY */ 4 +
        /* VelocityX */ 4 +
        /* VelocityY */ 4 +
        /* State */ 4 +
        /* Direction */ 4 +
        /* AnimationFrameIndex */ 4 +
        /* AnimationProgress */ 4;

    /// <summary>
    /// Packet sequence id for reliable ordering on top of unreliable channel (if used).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Server entity id (unique per entity on server).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.UInt32 EntityId { get; set; }

    /// <summary>
    /// Server timestamp in milliseconds (server clock).
    /// Clients should convert/align to their local clock offset when applying snapshots.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.UInt64 ServerTimestampMs { get; set; }

    /// <summary>
    /// World X position (float).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Single PositionX { get; set; }

    /// <summary>
    /// World Y position (float).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Single PositionY { get; set; }

    /// <summary>
    /// Velocity X (units per second), used for client extrapolation when necessary.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Single VelocityX { get; set; }

    /// <summary>
    /// Velocity Y (units per second), used for client extrapolation when necessary.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Single VelocityY { get; set; }

    /// <summary>
    /// Authoritative player state (Idle/Walking/Running/etc).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public PlayerState State { get; set; }

    /// <summary>
    /// Facing direction (Down/Up/Left/Right).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public Direction2D Direction { get; set; }

    /// <summary>
    /// Optional animation frame index. -1 indicates "not provided".
    /// If provided, client may snap animator to this frame index.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.Int32 AnimationFrameIndex { get; set; } = -1;

    /// <summary>
    /// Optional normalized animation progress (0.0 - 1.0).
    /// Use this when server prefers to send animation timing rather than frame index.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.Single AnimationProgress { get; set; } = 0f;

    /// <summary>
    /// Default constructor initializes magic/opcode and zeroed payload.
    /// </summary>
    public PlayerSnapshotPacket()
    {
        OpCode = OpCommand.NONE.AsUInt16();
        MagicNumber = PacketMagic.PLAYER_SNAPSHOT.AsUInt32();
        ResetForPool();
    }

    /// <summary>
    /// Initialize packet payload quickly.
    /// </summary>
    public void Initialize(
        System.UInt16 opCode,
        System.UInt32 sequenceId,
        System.UInt32 entityId,
        System.UInt64 serverTimestampMs,
        System.Single posX,
        System.Single posY,
        System.Single velX,
        System.Single velY,
        PlayerState state,
        Direction2D direction,
        System.Int32 animationFrameIndex = -1,
        System.Single animationProgress = 0f)
    {
        OpCode = opCode;
        SequenceId = sequenceId;
        EntityId = entityId;
        ServerTimestampMs = serverTimestampMs;
        PositionX = posX;
        PositionY = posY;
        VelocityX = velX;
        VelocityY = velY;
        State = state;
        Direction = direction;
        AnimationFrameIndex = animationFrameIndex;
        AnimationProgress = animationProgress;
    }

    /// <summary>
    /// Reset fields for object pooling reuse.
    /// </summary>
    public override void ResetForPool()
    {
        EntityId = 0;
        SequenceId = 0;
        PositionX = 0f;
        PositionY = 0f;
        VelocityX = 0f;
        VelocityY = 0f;
        ServerTimestampMs = 0;
        AnimationProgress = 0f;
        AnimationFrameIndex = -1;
        State = PlayerState.Idle;
        Direction = Direction2D.Down;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    /// <summary>
    /// Deserialize a packet from a byte buffer using LiteSerializer and pooled instance.
    /// </summary>
    public static PlayerSnapshotPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        PlayerSnapshotPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                              .Get<PlayerSnapshotPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);
}