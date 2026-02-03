// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Managers;
using SFML.Graphics;

namespace Ascendance.Sandbox;

/// <summary>
/// Manages assets specific to the Sandbox environment, such as fonts, textures, and sounds.
/// </summary>
public static class SandboxAssets
{
    /// <summary>
    /// Default font used in Sandbox environment.
    /// </summary>
    public static readonly Font DefaultFont = AssetManager.Instance.LoadFont("res/fonts/1.ttf");
}