// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Abstractions;

public interface IUpdatable
{
    /// <summary>
    /// Updates the object with the given delta time.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    void Update(System.Single deltaTime);
}
