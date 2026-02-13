// Copyright (c) 2026 Ascendance Team. All rights reserved.

namespace Ascendance.Contracts.Enums;

public enum PacketMagic : System.UInt32
{
    CHANGE_PASSWORD = 0x47414D45,
    RESPONSE = 0x4E414C49,
    PLAYER_SNAPSHOT = 0x54504C41,
    CREDENTIALS = 0x58494C41,
}