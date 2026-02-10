// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Ascendance.Domain.Tiles;

/// <summary>
/// Represents a single tile in the tile map with collision and rendering properties.
/// </summary>
/// <remarks>
/// This struct is optimized for cache-friendly memory layout with value semantics.
/// Uses struct for better performance in large tile arrays.
/// </remarks>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Tile : System.IEquatable<Tile>
{
    #region Fields

    /// <summary>
    /// The global tile ID from the tileset.
    /// </summary>
    public System.Int32 Gid;

    /// <summary>
    /// The local tile ID within the tileset (0-based).
    /// </summary>
    public System.Int32 LocalId;

    /// <summary>
    /// The texture rectangle for this tile in the tileset.
    /// </summary>
    public IntRect TextureRect;

    /// <summary>
    /// The world position in pixels.
    /// </summary>
    public Vector2f WorldPosition;

    /// <summary>
    /// Packed flags for tile properties (collision, flip, rotation).
    /// </summary>
    private System.Byte _flags;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets whether this tile blocks movement (collision).
    /// </summary>
    public readonly System.Boolean IsCollidable
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => (_flags & 0x01) != 0;
    }

    /// <summary>
    /// Gets or sets whether this tile is flipped horizontally.
    /// </summary>
    public readonly System.Boolean IsFlippedHorizontally
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => (_flags & 0x02) != 0;
    }

    /// <summary>
    /// Gets or sets whether this tile is flipped vertically.
    /// </summary>
    public readonly System.Boolean IsFlippedVertically
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => (_flags & 0x04) != 0;
    }

    /// <summary>
    /// Gets or sets whether this tile is flipped diagonally.
    /// </summary>
    public readonly System.Boolean IsFlippedDiagonally
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => (_flags & 0x08) != 0;
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> struct.
    /// </summary>
    /// <param name="gid">The global tile ID.</param>
    /// <param name="localId">The local tile ID.</param>
    /// <param name="textureRect">The texture rectangle.</param>
    /// <param name="worldPosition">The world position.</param>
    /// <param name="isCollidable">Whether the tile is collidable.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public Tile(
        System.Int32 gid,
        System.Int32 localId,
        IntRect textureRect,
        Vector2f worldPosition,
        System.Boolean isCollidable = false)
    {
        Gid = gid;
        LocalId = localId;
        TextureRect = textureRect;
        WorldPosition = worldPosition;
        _flags = (System.Byte)(isCollidable ? 0x01 : 0x00);
    }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Sets the collidable flag.
    /// </summary>
    /// <param name="value">Whether the tile should be collidable.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetCollidable(System.Boolean value)
    {
        if (value)
        {
            _flags |= 0x01;
        }
        else
        {
            _flags &= 0xFE;
        }
    }

    /// <summary>
    /// Sets the horizontal flip flag.
    /// </summary>
    /// <param name="value">Whether the tile should be flipped horizontally.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetFlippedHorizontally(System.Boolean value)
    {
        if (value)
        {
            _flags |= 0x02;
        }
        else
        {
            _flags &= 0xFD;
        }
    }

    /// <summary>
    /// Sets the vertical flip flag.
    /// </summary>
    /// <param name="value">Whether the tile should be flipped vertically.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetFlippedVertically(System.Boolean value)
    {
        if (value)
        {
            _flags |= 0x04;
        }
        else
        {
            _flags &= 0xFB;
        }
    }

    /// <summary>
    /// Sets the diagonal flip flag.
    /// </summary>
    /// <param name="value">Whether the tile should be flipped diagonally.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetFlippedDiagonally(System.Boolean value)
    {
        if (value)
        {
            _flags |= 0x08;
        }
        else
        {
            _flags &= 0xF7;
        }
    }

    /// <summary>
    /// Determines whether this tile is empty (has no graphic representation).
    /// </summary>
    /// <returns><c>true</c> if the tile has a GID of 0; otherwise, <c>false</c>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly System.Boolean IsEmpty() => Gid == 0;

    /// <summary>
    /// Creates an empty tile at the specified world position.
    /// </summary>
    /// <param name="worldPosition">The world position for the empty tile.</param>
    /// <returns>An empty tile instance.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static Tile CreateEmpty(Vector2f worldPosition) =>
        new(0, -1, default, worldPosition, false);

    #endregion Methods

    #region IEquatable

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public readonly System.Boolean Equals(Tile other) =>
        Gid == other.Gid &&
        LocalId == other.LocalId &&
        WorldPosition.X == other.WorldPosition.X &&
        WorldPosition.Y == other.WorldPosition.Y;

    /// <inheritdoc/>
    public override readonly System.Boolean Equals(System.Object obj) =>
        obj is Tile other && Equals(other);

    /// <inheritdoc/>
    public override readonly System.Int32 GetHashCode() =>
        System.HashCode.Combine(Gid, LocalId, WorldPosition.X, WorldPosition.Y);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static System.Boolean operator ==(Tile left, Tile right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static System.Boolean operator !=(Tile left, Tile right) => !left.Equals(right);

    #endregion IEquatable
}