// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Entities;
using Ascendance.Shared.Enums;
using Ascendance.Shared.Map;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tilemap;

/// <summary>
/// Responsible for rendering a <see cref="TileMap"/> using tile textures and batching for performance.
/// Supports both full map rendering and camera-based visible-region rendering.
/// </summary>
public class TileMapRenderer
{
    #region Fields

    private readonly TileMap _map;
    private readonly SpriteBatch[] _batches;
    private readonly Texture[] _tileTextures;
    private readonly System.Int32 _tileWidth;
    private readonly System.Int32 _tileHeight;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TileMapRenderer"/> class.
    /// </summary>
    /// <param name="tileTextures">
    /// An array of <see cref="Texture"/> objects representing all tile graphics.
    /// Each index corresponds to a tile type.
    /// </param>
    /// <param name="tileWidth">
    /// The width (in pixels) of each tile.
    /// </param>
    /// <param name="tileHeight">
    /// The height (in pixels) of each tile.
    /// </param>
    /// <param name="map">
    /// The <see cref="TileMap"/> instance to render.
    /// </param>
    public TileMapRenderer(Texture[] tileTextures, System.Int32 tileWidth, System.Int32 tileHeight, TileMap map)
    {
        _map = map;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _tileTextures = tileTextures;
        _batches = new SpriteBatch[_tileTextures.Length];

        for (System.Int32 i = 0; i < _tileTextures.Length; i++)
        {
            _batches[i] = new SpriteBatch(_tileTextures[i]);
        }
        for (System.UInt32 y = 0; y < _map.Height; y++)
        {
            for (System.UInt32 x = 0; x < _map.Width; x++)
            {
                Tile tile = _map.GetTile((System.Int32)x, (System.Int32)y);
                if (tile.Type == TileType.Unknown)
                {
                    continue;
                }

                System.Int32 id = (System.Int32)tile.Type;
                System.Int32 cols = (System.Int32)_tileTextures[id].Size.X / _tileWidth;
                System.Int32 tx = id % cols, ty = id / cols;
                IntRect src = new(tx * _tileWidth, ty * _tileHeight, _tileWidth, _tileHeight);

                _batches[id].Add(new Vector2f(x * _tileWidth, y * _tileHeight), src);
            }
        }
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Draws the entire tile map to the specified render target.
    /// </summary>
    /// <param name="target">
    /// The <see cref="RenderTarget"/> onto which the tile map will be drawn (e.g., window or buffer).
    /// </param>
    public void Draw(RenderTarget target)
    {
        foreach (var batch in _batches)
        {
            batch?.Draw(target);
        }
    }

    /// <summary>
    /// Draws only the visible region of the tile map that is within the view of the specified <see cref="Camera2D"/>.
    /// This method performs batching per frame for the current view.
    /// </summary>
    /// <param name="target">
    /// The <see cref="RenderTarget"/> onto which the (visible region of) tile map will be drawn.
    /// </param>
    /// <param name="camera">
    /// The <see cref="Camera2D"/> controlling the current view and determining which tiles are visible.
    /// </param>
    public void Draw(RenderTarget target, Camera2D camera)
    {
        // Lấy view của camera
        View view = camera.SFMLView;
        Vector2f center = view.Center;
        Vector2f size = view.Size;

        // Tính ra góc trên trái và dưới phải của camera theo world
        Vector2f topLeft = new(center.X - (size.X / 2), center.Y - (size.Y / 2));
        Vector2f bottomRight = new(center.X + (size.X / 2), center.Y + (size.Y / 2));

        // Đổi vùng world ra index tile, giới hạn trong map
        System.Int32 minTileX = System.Math.Max(0, (System.Int32)(topLeft.X / _tileWidth));
        System.Int32 minTileY = System.Math.Max(0, (System.Int32)(topLeft.Y / _tileHeight));
        System.Int32 maxTileX = System.Math.Min(_map.Width - 1, (System.Int32)(bottomRight.X / _tileWidth));
        System.Int32 maxTileY = System.Math.Min(_map.Height - 1, (System.Int32)(bottomRight.Y / _tileHeight));

        // Clear batch cho frame mới (nếu batch động theo vùng nhìn thấy)
        foreach (SpriteBatch batch in _batches)
        {
            batch?.Clear();
        }

        // Batch lại tile nằm trên vùng camera thấy
        for (System.Int32 y = minTileY; y <= maxTileY; y++)
        {
            for (System.Int32 x = minTileX; x <= maxTileX; x++)
            {
                Tile tile = _map.GetTile(x, y);
                if (tile.Type == TileType.Unknown)
                {
                    continue;
                }

                System.Int32 id = (System.Int32)tile.Type;
                System.Int32 cols = (System.Int32)_tileTextures[id].Size.X / _tileWidth;
                System.Int32 tx = id % cols, ty = id / cols;
                IntRect src = new(tx * _tileWidth, ty * _tileHeight, _tileWidth, _tileHeight);

                _batches[id].Add(new Vector2f(x * _tileWidth, y * _tileHeight), src);
            }
        }

        // Vẽ các batch
        foreach (SpriteBatch batch in _batches)
        {
            batch?.Draw(target);
        }
    }

    #endregion Public Methods
}