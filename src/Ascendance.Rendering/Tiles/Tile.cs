// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tiles;

/// <summary>
/// Represents a single tile in the tile map with collision and rendering properties.
/// </summary>
/// <remarks>
/// This class encapsulates all data needed to render and process a tile, including
/// its position, texture coordinates, collision state, and custom properties from Tiled.
/// </remarks>
public sealed class Tile
{
    #region Properties

    /// <summary>
    /// Gets or sets the global tile ID from the tileset.
    /// </summary>
    /// <value>
    /// The global identifier used to reference this tile type across the entire tileset.
    /// A value of 0 indicates an empty tile.
    /// </value>
    public System.Int16 Gid { get; set; }

    /// <summary>
    /// Gets or sets the local tile ID within the tileset (0-based).
    /// </summary>
    /// <value>
    /// The zero-based index of this tile within its parent tileset.
    /// A value of -1 indicates the tile has not been assigned a local ID.
    /// </value>
    public System.Int16 LocalId { get; set; }

    /// <summary>
    /// Gets or sets the texture rectangle for this tile in the tileset.
    /// </summary>
    /// <value>
    /// The rectangular region in the tileset texture that contains this tile's graphic data.
    /// </value>
    public IntRect TextureRect { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this tile blocks movement (collision).
    /// </summary>
    /// <value>
    /// <c>true</c> if this tile should block player or entity movement; otherwise, <c>false</c>.
    /// </value>
    public System.Boolean IsCollidable { get; set; }

    /// <summary>
    /// Gets or sets the X position in tile coordinates.
    /// </summary>
    /// <value>
    /// The horizontal position of this tile in the tile grid coordinate system.
    /// </value>
    public System.Int16 X { get; set; }

    /// <summary>
    /// Gets or sets the Y position in tile coordinates.
    /// </summary>
    /// <value>
    /// The vertical position of this tile in the tile grid coordinate system.
    /// </value>
    public System.Int16 Y { get; set; }

    /// <summary>
    /// Gets or sets the world position in pixels.
    /// </summary>
    /// <value>
    /// The actual pixel coordinates of this tile in world space, calculated from tile coordinates.
    /// </value>
    public Vector2f WorldPosition { get; set; }

    /// <summary>
    /// Gets or sets the custom properties from Tiled editor.
    /// </summary>
    /// <value>
    /// A dictionary of key-value pairs containing custom metadata defined in the Tiled map editor.
    /// This is never <c>null</c> after construction.
    /// </value>
    public System.Collections.Generic.Dictionary<System.String, System.String> Properties { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class with default values.
    /// </summary>
    /// <remarks>
    /// Creates an empty, non-collidable tile at position (0,0) with no assigned local ID.
    /// The <see cref="Properties"/> dictionary is initialized as empty but non-null.
    /// </remarks>
    public Tile()
    {
        Gid = 0;
        LocalId = -1;
        Properties = [];
        IsCollidable = false;
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Determines whether this tile is empty (has no graphic representation).
    /// </summary>
    /// <returns>
    /// <c>true</c> if the tile has a <see cref="Gid"/> of 0, indicating no graphic; 
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is marked with <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance in tight rendering loops.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsEmpty() => Gid == 0;

    #endregion Methods
}