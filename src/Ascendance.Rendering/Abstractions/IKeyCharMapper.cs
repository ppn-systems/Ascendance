// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Abstractions;

/// <summary>
/// Defines a service that maps keyboard key input to a character representation,
/// taking modifier states (such as Shift) into account.
/// </summary>
/// <remarks>
/// Implementations are typically keyboard-layout dependent (e.g. US, JP, VN)
/// and are used for text input and UI rendering systems.
/// </remarks>
public interface IKeyCharMapper
{
    /// <summary>
    /// Attempts to map the current key input to a character.
    /// </summary>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the mapped character.
    /// Otherwise, contains <see langword="default"/>.
    /// </param>
    /// <param name="shift">
    /// Indicates whether the Shift modifier key is currently active.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key was successfully mapped to a character;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    System.Boolean TryMapKeyToChar(out System.Char result, System.Boolean shift);
}
