// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Maps.Layers;

/// <summary>
/// Represents a tile instance in a tile layer including position and flip flags.
/// </summary>
public class TmxLayerTile
{
    #region Constants

    // Tile flip bit flags (as defined by the TMX / Tiled specification)
    private const System.UInt32 FLIPPED_VERTICALLY_FLAG = 0x4000_0000;
    private const System.UInt32 FLIPPED_DIAGONALLY_FLAG = 0x2000_0000;
    private const System.UInt32 FLIPPED_HORIZONTALLY_FLAG = 0x8000_0000;

    #endregion Constants

    #region Properties

    /// <summary>
    /// Global tile ID (GID) with flip bits removed.
    /// </summary>
    public System.Int32 Gid { get; }

    /// <summary>
    /// Tile column (x) in tiles.
    /// </summary>
    public System.Int32 X { get; }

    /// <summary>
    /// Tile row (y) in tiles.
    /// </summary>
    public System.Int32 Y { get; }

    /// <summary>
    /// Whether the tile is flipped horizontally.
    /// </summary>
    public System.Boolean HorizontalFlip { get; }

    /// <summary>
    /// Whether the tile is flipped vertically.
    /// </summary>
    public System.Boolean VerticalFlip { get; }

    /// <summary>
    /// Whether the tile is flipped diagonally.
    /// </summary>
    public System.Boolean DiagonalFlip { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="TmxLayerTile"/> by decoding the raw 32-bit TMX tile id.
    /// The high three bits represent flip flags and are removed when computing <see cref="Gid"/>.
    /// </summary>
    /// <param name="id">Raw 32-bit TMX id (includes flip bits).</param>
    /// <param name="x">Tile x position (column).</param>
    /// <param name="y">Tile y position (row).</param>
    /// <exception cref="System.OverflowException">Thrown if the decoded GID does not fit into a signed 32-bit integer.</exception>
    public TmxLayerTile(System.UInt32 id, System.Int32 x, System.Int32 y)
    {
        X = x;
        Y = y;

        // Extract flip flags
        HorizontalFlip = (id & FLIPPED_HORIZONTALLY_FLAG) != 0;
        VerticalFlip = (id & FLIPPED_VERTICALLY_FLAG) != 0;
        DiagonalFlip = (id & FLIPPED_DIAGONALLY_FLAG) != 0;

        // Clear flag bits to obtain the actual GID
        System.UInt32 rawGid = id & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

        // Ensure the remaining value fits into Int32
        if (rawGid > System.Int32.MaxValue)
        {
            throw new System.OverflowException("Decoded GID is too large to fit into Int32.");
        }

        Gid = (System.Int32)rawGid;
    }

    #endregion Constructor

    #region Private Methods

    /// <summary>
    /// Decodes the given raw TMX id into its components without creating an instance.
    /// Useful when you only need the decoded values.
    /// </summary>
    /// <param name="id">Raw 32-bit TMX id (includes flip bits).</param>
    /// <param name="gid">Decoded GID (flip bits removed).</param>
    /// <param name="horizontalFlip">Horizontal flip flag.</param>
    /// <param name="verticalFlip">Vertical flip flag.</param>
    /// <param name="diagonalFlip">Diagonal flip flag.</param>
    public static void DECODE_GID(System.UInt32 id, out System.Int32 gid, out System.Boolean horizontalFlip, out System.Boolean verticalFlip, out System.Boolean diagonalFlip)
    {
        horizontalFlip = (id & FLIPPED_HORIZONTALLY_FLAG) != 0;
        verticalFlip = (id & FLIPPED_VERTICALLY_FLAG) != 0;
        diagonalFlip = (id & FLIPPED_DIAGONALLY_FLAG) != 0;

        System.UInt32 rawGid = id & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

        if (rawGid > System.Int32.MaxValue)
        {
            throw new System.OverflowException("Decoded GID is too large to fit into Int32.");
        }

        gid = (System.Int32)rawGid;
    }

    #endregion Private Methods
}