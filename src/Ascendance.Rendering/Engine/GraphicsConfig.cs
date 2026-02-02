// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Core.Attributes;
using Nalix.Framework.Configuration.Binding;

namespace Ascendance.Rendering.Engine;

/// <summary>
/// Represents the configuration for the graphics assembly in the Nalix framework.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Screen={ScreenWidth}x{ScreenHeight}, VSync={VSync}, Volume={MusicVolume}/{SoundVolume}")]
public sealed class GraphicsConfig : ConfigurationLoader
{
    #region Constants

    /// <summary>
    /// Gets the base path for assets. Default value is the current domain's base directory.
    /// </summary>
    [ConfiguredIgnore]
    public static System.String AssetRoot { get; } = System.AppDomain.CurrentDomain.BaseDirectory;

    #endregion Constants

    #region Properties

    /// <summary>
    /// Gets a value indicating whether VSync is enabled. Default value is false.
    /// </summary>
    public System.Boolean VSync { get; init; } = false;

    /// <summary>
    /// Gets the frame limit for the application. Default value is 60.
    /// </summary>
    public System.UInt32 FrameLimit { get; init; } = 60;

    /// <summary>
    /// Gets the music volume, ranging from 0 (mute) to 100 (maximum). Default value is 50.
    /// </summary>
    public System.Single MusicVolume { get; init; } = 50;

    /// <summary>
    /// Gets the sound volume, ranging from 0 (mute) to 100 (maximum). Default value is 100.
    /// </summary>
    public System.Single SoundVolume { get; init; } = 100;

    /// <summary>
    /// Gets the width of the screen in pixels. Default value is 1280.
    /// </summary>
    public System.UInt32 ScreenWidth { get; init; } = 1280;

    /// <summary>
    /// Gets the height of the screen in pixels. Default value is 720.
    /// </summary>
    public System.UInt32 ScreenHeight { get; init; } = 720;

    /// <summary>
    /// Gets the title of the application window. Default value is "Ascendance".
    /// </summary>
    public System.String Title { get; init; } = "Ascendance";

    /// <summary>
    /// Gets the name of the main scene to be loaded. Default value is "main".
    /// </summary>
    public System.String MainScene { get; init; } = "main";

    /// <summary>
    /// Gets the namespace where scenes are located. Default value is "Scenes".
    /// </summary>
    public System.String SceneNamespace { get; init; } = "Scenes";

    #endregion Properties
}