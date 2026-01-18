// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Entities;
using Ascendance.Shared.Enums;
using Ascendance.Shared.Map;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tilemap;

public class TileMapRenderer
{
    private readonly TileMap _map;
    private readonly SpriteBatch[] _batches;
    private readonly Texture[] _tileTextures;
    private readonly System.Int32 _tileWidth;
    private readonly System.Int32 _tileHeight;

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

    public void Draw(RenderTarget target)
    {
        foreach (var batch in _batches)
        {
            batch?.Draw(target);
        }
    }

    /// <summary>
    /// Draws only the visible region of TileMap within the Camera2D's view.
    /// </summary>
    /// <param name="target">The render target (window/screen).</param>
    /// <param name="camera">The current camera controlling view.</param>
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
}