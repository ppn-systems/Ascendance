// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Abstractions;

/// <summary>
/// Represents an object that can receive and lose focus within the UI or rendering environment.
/// </summary>
public interface IFocusable
{
    /// <summary>
    /// Called when the object loses focus.
    /// </summary>
    void OnFocusLost();

    /// <summary>
    /// Called when the object receives focus.
    /// </summary>
    void OnFocusGained();
}