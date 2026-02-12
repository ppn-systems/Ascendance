// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Tiled.Tilesets;

/// <summary>
/// Represents a single animation frame inside a &lt;animation&gt; block for a tile.
/// </summary>
public class TmxAnimationFrame
{
    #region Properties

    /// <summary>
    /// Local tile id for this frame.
    /// </summary>
    public System.Int32 Id { get; }

    /// <summary>
    /// Duration of the frame in milliseconds.
    /// </summary>
    public System.Int32 Duration { get; }

    #endregion Properties

    #region Constructor

    public TmxAnimationFrame(System.Xml.Linq.XElement xFrame)
    {
        System.ArgumentNullException.ThrowIfNull(xFrame);

        Id = (System.Int32?)xFrame.Attribute("tileid") ?? 0;
        Duration = (System.Int32?)xFrame.Attribute("duration") ?? 0;
    }

    #endregion Constructor
}