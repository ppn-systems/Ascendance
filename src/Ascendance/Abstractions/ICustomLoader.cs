// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Core;

namespace Ascendance.Abstractions;

/// <summary>
/// Interface for a custom XML loader used by <see cref="TmxDocument"/>.
/// </summary>
public interface ICustomLoader
{
    System.Xml.Linq.XDocument ReadXml(System.String filepath);
}
