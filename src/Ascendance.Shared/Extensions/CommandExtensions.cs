// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Shared.Enums;

namespace Ascendance.Shared.Extensions;

/// <summary>
/// Provides extension methods for command-related enum conversions.
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Converts an <see cref="OpCommand"/> enumeration value to its <see cref="System.UInt16"/> representation.
    /// </summary>
    /// <param name="command">The <see cref="OpCommand"/> enumeration value to convert.</param>
    /// <returns>A <see cref="System.UInt16"/> value that represents the specified <paramref name="command"/>.</returns>
    /// <example>
    /// <code>
    /// OpCommand cmd = OpCommand.Move;
    /// ushort value = cmd.AsUInt16();
    /// </code>
    /// </example>
    public static System.UInt16 AsUInt16(this OpCommand command) => (System.UInt16)command;

    /// <summary>
    /// Converts a <see cref="PacketMagic"/> enumeration value to its <see cref="System.UInt32"/> representation.
    /// </summary>
    /// <param name="command">The <see cref="PacketMagic"/> enumeration value to convert.</param>
    /// <returns>A <see cref="System.UInt32"/> value that represents the specified <paramref name="command"/>.</returns>
    /// <example>
    /// <code>
    /// PacketMagic magic = PacketMagic.Standard;
    /// uint value = magic.AsUInt32();
    /// </code>
    /// </example>
    public static System.UInt32 AsUInt32(this PacketMagic command) => (System.UInt32)command;
}