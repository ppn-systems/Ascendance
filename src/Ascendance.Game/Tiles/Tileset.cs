// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Ascendance.Game.Tiles;

/// <summary>
/// Represents a tileset containing tile graphics and metadata for use in tile-based rendering.
/// </summary>
/// <remarks>
/// <para>
/// A tileset is a collection of tile graphics stored in a single texture atlas image.
/// Each tile is identified by a local ID (relative to the tileset) and a global ID (GID) that
/// is unique across all tilesets in a tile map.
/// </para>
/// <para>
/// This class handles texture coordinate calculations for individual tiles, accounting for
/// spacing and margins in the tileset image. It is typically loaded from Tiled map editor (.tmx/.tsx) files.
/// </para>
/// </remarks>
public sealed class Tileset
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of the tileset.
    /// </summary>
    /// <value>
    /// The human-readable name of the tileset, typically defined in the Tiled map editor.
    /// This is used for identification and debugging purposes.
    /// </value>
    public System.String Name { get; set; }

    /// <summary>
    /// Gets or sets the first global ID (GID) of this tileset.
    /// </summary>
    /// <value>
    /// The starting global identifier for tiles in this tileset. All tiles in this tileset
    /// have GIDs in the range [<see cref="FirstGid"/>, <see cref="FirstGid"/> + <see cref="TileCount"/>).
    /// This value is used to map global tile IDs to local tile IDs within the tileset.
    /// </value>
    /// <remarks>
    /// In Tiled map format, the FirstGid is assigned based on the order tilesets are added to the map.
    /// The first tileset typically has FirstGid = 1 (since GID 0 is reserved for empty tiles).
    /// </remarks>
    public System.Int16 FirstGid { get; set; }

    /// <summary>
    /// Gets or sets the width of each tile in pixels.
    /// </summary>
    /// <value>
    /// The horizontal size of a single tile in the tileset. Must be greater than zero.
    /// Common values are 16, 32, or 64 pixels.
    /// </value>
    public System.Int16 TileWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of each tile in pixels.
    /// </summary>
    /// <value>
    /// The vertical size of a single tile in the tileset. Must be greater than zero.
    /// Common values are 16, 32, or 64 pixels.
    /// </value>
    public System.Int16 TileHeight { get; set; }

    /// <summary>
    /// Gets or sets the number of tile columns in the tileset image.
    /// </summary>
    /// <value>
    /// The number of tiles horizontally across the tileset texture.
    /// Used to calculate tile positions in the texture atlas.
    /// </value>
    public System.Int16 Columns { get; set; }

    /// <summary>
    /// Gets or sets the total number of tiles in this tileset.
    /// </summary>
    /// <value>
    /// The count of all tiles available in this tileset. This determines the valid range
    /// of local IDs (0 to <see cref="TileCount"/> - 1) and the upper bound of GIDs
    /// (<see cref="FirstGid"/> + <see cref="TileCount"/>).
    /// </value>
    public System.Int16 TileCount { get; set; }

    /// <summary>
    /// Gets or sets the spacing between tiles in pixels.
    /// </summary>
    /// <value>
    /// The gap between adjacent tiles in the tileset image. Default is 0.
    /// This value is used in texture coordinate calculations via <see cref="GetTileRect"/>.
    /// </value>
    /// <remarks>
    /// Some tileset images include spacing to prevent texture bleeding artifacts
    /// when rendering with certain filtering modes.
    /// </remarks>
    public System.Int16 Spacing { get; set; }

    /// <summary>
    /// Gets or sets the margin around the tileset in pixels.
    /// </summary>
    /// <value>
    /// The border size around the entire tileset image before the first tile starts. Default is 0.
    /// This value offsets all tile position calculations in <see cref="GetTileRect"/>.
    /// </value>
    /// <remarks>
    /// Margins are common in tileset images exported from certain graphics editors.
    /// </remarks>
    public System.Int16 Margin { get; set; }

    /// <summary>
    /// Gets or sets the file path to the tileset image.
    /// </summary>
    /// <value>
    /// The relative or absolute path to the PNG or other image file containing the tileset graphics.
    /// This path is typically used to load the <see cref="Texture"/>.
    /// </value>
    public System.String ImagePath { get; set; }

    /// <summary>
    /// Gets or sets the SFML texture loaded from the tileset image.
    /// </summary>
    /// <value>
    /// The <see cref="SFML.Graphics.Texture"/> object containing the tileset graphics.
    /// May be <c>null</c> if the texture has not been loaded yet or failed to load.
    /// </value>
    /// <remarks>
    /// This texture is used during rendering to draw individual tiles via their texture rectangles
    /// obtained from <see cref="GetTileRect"/>.
    /// </remarks>
    public Texture Texture { get; set; }

    /// <summary>
    /// Gets or sets the custom properties for individual tiles indexed by local tile ID.
    /// </summary>
    /// <value>
    /// A dictionary where the key is the local tile ID (0-based) and the value is another dictionary
    /// of property key-value pairs defined in the Tiled map editor.
    /// This collection is never <c>null</c> after construction but may be empty.
    /// </value>
    /// <remarks>
    /// Tile properties are used to store metadata such as collision flags, animation data,
    /// or gameplay-specific information defined in the Tiled editor.
    /// </remarks>
    public System.Collections.Generic.Dictionary<System.Int32, System.Collections.Generic.Dictionary<System.String, System.String>> TileProperties { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Tileset"/> class with default values.
    /// </summary>
    /// <remarks>
    /// Creates a tileset with no spacing or margin and an empty tile properties collection.
    /// The <see cref="Texture"/> and <see cref="ImagePath"/> are not initialized and must be set separately.
    /// </remarks>
    public Tileset()
    {
        Spacing = 0;
        Margin = 0;
        TileProperties = [];
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Calculates the texture rectangle for a specific tile by its local ID.
    /// </summary>
    /// <param name="localId">The local tile ID (0-based index within this tileset).</param>
    /// <returns>
    /// An <see cref="IntRect"/> representing the texture coordinates (in pixels) of the tile
    /// within the tileset image. The rectangle includes the tile's position (X, Y) and dimensions
    /// (<see cref="TileWidth"/>, <see cref="TileHeight"/>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method calculates the tile's position in the texture atlas using the following formula:
    /// </para>
    /// <code>
    /// row = localId / Columns
    /// col = localId % Columns
    /// x = Margin + (col × (TileWidth + Spacing))
    /// y = Margin + (row × (TileHeight + Spacing))
    /// </code>
    /// <para>
    /// The calculation accounts for <see cref="Margin"/> and <see cref="Spacing"/> to correctly
    /// locate tiles in tilesets with gaps or borders. This method is marked with
    /// <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance during tile rendering.
    /// </para>
    /// </remarks>
    /// <exception cref="System.DivideByZeroException">
    /// Thrown if <see cref="Columns"/> is 0.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public IntRect GetTileRect(System.Int32 localId)
    {
        System.Int32 row = localId / Columns;
        System.Int32 col = localId % Columns;

        System.Int32 x = Margin + (col * (TileWidth + Spacing));
        System.Int32 y = Margin + (row * (TileHeight + Spacing));

        return new IntRect(x, y, TileWidth, TileHeight);
    }

    /// <summary>
    /// Converts a global tile ID (GID) to a local tile ID for this tileset.
    /// </summary>
    /// <param name="gid">The global tile identifier to convert.</param>
    /// <returns>
    /// The local tile ID (0-based) within this tileset if the GID belongs to this tileset;
    /// otherwise, -1 if the GID is outside the valid range for this tileset.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The conversion is performed using the formula:
    /// </para>
    /// <code>
    /// localId = gid - FirstGid
    /// </code>
    /// <para>
    /// A GID is considered valid for this tileset if it falls within the range
    /// [<see cref="FirstGid"/>, <see cref="FirstGid"/> + <see cref="TileCount"/>).
    /// This method is marked with <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance in tile rendering pipelines.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Int16 GidToLocalId(System.Int16 gid) => (System.Int16)(gid < FirstGid || gid >= FirstGid + TileCount ? -1 : gid - FirstGid);

    /// <summary>
    /// Determines whether a global tile ID (GID) belongs to this tileset.
    /// </summary>
    /// <param name="gid">The global tile identifier to check.</param>
    /// <returns>
    /// <c>true</c> if the GID is within the valid range for this tileset
    /// ([<see cref="FirstGid"/>, <see cref="FirstGid"/> + <see cref="TileCount"/>));
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is commonly used to find the correct tileset for a given GID when
    /// multiple tilesets are present in a tile map. It is marked with
    /// <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
    /// for optimal performance in tileset lookup operations.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean ContainsGid(System.Int32 gid) => gid >= FirstGid && gid < FirstGid + TileCount;

    /// <summary>
    /// Gets the tile properties dictionary for a specific tile, creating it if it doesn't exist.
    /// </summary>
    /// <param name="localId">The local tile ID (0-based index within this tileset).</param>
    /// <returns>
    /// The properties dictionary for the specified tile. Never returns <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful when loading TMX files, as it allows properties to be added
    /// to tiles without checking if the dictionary exists first.
    /// </remarks>
    public System.Collections.Generic.Dictionary<System.String, System.String> GetOrCreateTileProperties(System.Int32 localId)
    {
        if (!TileProperties.TryGetValue(localId, out System.Collections.Generic.Dictionary<System.String, System.String> props))
        {
            props = new System.Collections.Generic.Dictionary<System.String, System.String>(System.StringComparer.Ordinal);
            TileProperties[localId] = props;
        }

        return props;
    }

    #endregion Methods
}