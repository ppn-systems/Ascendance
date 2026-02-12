// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Maps.Abstractions;

/// <summary>
/// Generic small interface for TMX elements that expose a name.
/// </summary>
public interface ITmxElement
{
    System.String Name { get; }
}
